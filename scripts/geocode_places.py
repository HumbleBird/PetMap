#!/usr/bin/env python3
"""Match spreadsheet place rows to Kakao Local keyword-search results.

Reads KAKAO_REST_API_KEY from the environment. It never prints the key.
Writes a review CSV; it does not write to any database.
"""

from __future__ import annotations

import csv
import os
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from difflib import SequenceMatcher
from pathlib import Path

from openpyxl import load_workbook


DEFAULT_INPUT = Path(r"D:\Download\리스트분할.xlsx")
DEFAULT_OUTPUT = Path("artifacts/place-import-review.csv")
CATEGORIES = {
    "카페": "BakeryCafe",
    "식당": "Restaurant",
    "병원": "Hospital",
    "공원": "Park",
}


def clean(value: object) -> str:
    return str(value or "").strip()


def normalize(value: str) -> str:
    value = re.sub(r"^(대한민국|경기도|경기|서울특별시|서울)\s*", "", value)
    return re.sub(r"[^0-9A-Za-z가-힣]", "", value).lower()


def address_similarity(source: str, candidate: str) -> float:
    a, b = normalize(source), normalize(candidate)
    if not a or not b:
        return 0.0
    if a in b or b in a:
        return 1.0
    return SequenceMatcher(None, a, b).ratio()


def read_rows(path: Path) -> list[dict[str, str]]:
    workbook = load_workbook(path, read_only=True, data_only=True)
    rows: list[dict[str, str]] = []
    for sheet in workbook.worksheets:
        category = CATEGORIES.get(sheet.title.strip())
        if not category:
            continue
        values = sheet.iter_rows(values_only=True)
        first = next(values, None)
        if first is None:
            continue
        first_cells = [clean(value) for value in first]
        has_header = any(token in " ".join(first_cells[:3]) for token in ("가게명", "상호명", "주소"))
        data_rows = values if has_header else iter([first, *values])
        for row in data_rows:
            cells = [clean(value) for value in row]
            if len(cells) < 2 or not cells[0] or not cells[1]:
                continue
            phone = cells[2] if len(cells) > 2 and cells[2] not in ("-", "없음", "미정") else ""
            note = cells[3] if len(cells) > 3 else ""
            rows.append({
                "source_sheet": sheet.title,
                "name": cells[0],
                "source_address": cells[1],
                "phone": phone,
                "note": note,
                "category": category,
            })
    workbook.close()
    return rows


def kakao_search(name: str, api_key: str) -> list[dict[str, object]]:
    query = urllib.parse.urlencode({"query": name, "size": 15})
    request = urllib.request.Request(
        f"https://dapi.kakao.com/v2/local/search/keyword.json?{query}",
        headers={"Authorization": f"KakaoAK {api_key}"},
    )
    with urllib.request.urlopen(request, timeout=20) as response:
        import json
        return json.loads(response.read().decode("utf-8")).get("documents", [])


def match(row: dict[str, str], results: list[dict[str, object]]) -> dict[str, object]:
    source_name = normalize(row["name"])
    ranked = []
    for place in results:
        result_name = clean(place.get("place_name"))
        result_address = clean(place.get("road_address_name")) or clean(place.get("address_name"))
        name_score = SequenceMatcher(None, source_name, normalize(result_name)).ratio()
        addr_score = max(
            address_similarity(row["source_address"], clean(place.get("road_address_name"))),
            address_similarity(row["source_address"], clean(place.get("address_name"))),
        )
        ranked.append((name_score, addr_score, place, result_address))
    ranked.sort(key=lambda item: (item[0] * 0.55 + item[1] * 0.45, item[0]), reverse=True)
    if not ranked:
        return {"match_status": "not_found"}
    name_score, addr_score, place, result_address = ranked[0]
    exact_name = normalize(clean(place.get("place_name"))) == source_name
    status = "matched" if exact_name and addr_score >= 0.45 else "review"
    return {
        "match_status": status,
        "matched_name": clean(place.get("place_name")),
        "matched_address": result_address,
        "latitude": clean(place.get("y")),
        "longitude": clean(place.get("x")),
        "kakao_phone": clean(place.get("phone")),
        "kakao_category": clean(place.get("category_name")),
        "kakao_url": clean(place.get("place_url")),
        "name_similarity": f"{name_score:.3f}",
        "address_similarity": f"{addr_score:.3f}",
    }


def main() -> int:
    api_key = os.environ.get("KAKAO_REST_API_KEY", "").strip()
    if not api_key:
        print("KAKAO_REST_API_KEY 환경 변수가 없습니다. 키를 채팅에 붙이지 마세요.", file=sys.stderr)
        return 2
    input_path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_INPUT
    output_path = Path(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_OUTPUT
    if not input_path.is_file():
        print(f"엑셀 파일을 찾을 수 없습니다: {input_path}", file=sys.stderr)
        return 2

    rows = read_rows(input_path)
    fields = ["source_sheet", "name", "source_address", "phone", "note", "category",
              "match_status", "matched_name", "matched_address", "latitude", "longitude",
              "kakao_phone", "kakao_category", "kakao_url", "name_similarity", "address_similarity"]
    output_path.parent.mkdir(parents=True, exist_ok=True)
    counts = {"matched": 0, "review": 0, "not_found": 0}
    try:
        with output_path.open("w", newline="", encoding="utf-8-sig") as output:
            writer = csv.DictWriter(output, fieldnames=fields)
            writer.writeheader()
            for index, row in enumerate(rows, start=1):
                try:
                    result = match(row, kakao_search(row["name"], api_key))
                except (urllib.error.URLError, TimeoutError, ValueError) as exc:
                    result = {"match_status": "api_error"}
                    print(f"[{index}/{len(rows)}] {row['name']}: API 오류 ({type(exc).__name__})")
                counts[result.get("match_status", "not_found")] = counts.get(result.get("match_status", "not_found"), 0) + 1
                writer.writerow({**row, **result})
                print(f"[{index}/{len(rows)}] {row['name']}: {result.get('match_status', 'not_found')}")
                time.sleep(0.15)
    except urllib.error.HTTPError as exc:
        print(f"카카오 API 요청 실패 (HTTP {exc.code}). 키 종류와 앱 권한을 확인하세요.", file=sys.stderr)
        return 1

    print(f"결과 파일: {output_path.resolve()}")
    print(f"매칭 {counts.get('matched', 0)}건 / 검토 {counts.get('review', 0)}건 / 미검색 {counts.get('not_found', 0)}건")
    print("이 스크립트는 CSV만 만들며 데이터베이스를 수정하지 않습니다.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
