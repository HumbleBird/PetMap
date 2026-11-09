#!/usr/bin/env python3
"""Build an additive SQL Server import from reviewed Kakao matches."""

import csv
import sys
from pathlib import Path


SOURCE = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("artifacts/place-import-review.csv")
OUTPUT = Path(sys.argv[2]) if len(sys.argv) > 2 else Path("artifacts/import_matched_places.sql")


def literal(value: str) -> str:
    return "N'" + value.replace("'", "''") + "'"


with SOURCE.open(encoding="utf-8-sig", newline="") as source:
    rows = [row for row in csv.DictReader(source) if row.get("match_status") == "matched"]

lines = [
    "SET XACT_ABORT ON;",
    "IF COL_LENGTH('dbo.Places', 'Phone') IS NULL",
    "    THROW 51000, 'Deploy the latest PetMap schema before importing place rows.', 1;",
    "BEGIN TRANSACTION;",
    "",
    "-- Add only; existing rows are left unchanged.",
]
for row in rows:
    name = literal(row["name"])
    address = literal(row["source_address"])
    road_address = literal(row["matched_address"])
    latitude = str(float(row["latitude"]))
    longitude = str(float(row["longitude"]))
    category = literal(row["category"])
    phone = literal(row["phone"]) if row["phone"] else "NULL"
    description = literal(row["note"]) if row["note"] else "NULL"
    lines.extend([
        f"IF NOT EXISTS (SELECT 1 FROM dbo.Places WHERE Name = {name} AND (Address = {address} OR RoadAddress = {road_address}))",
        "BEGIN",
        "    INSERT INTO dbo.Places (Name, Address, RoadAddress, Latitude, Longitude, Category, Phone, Description)",
        f"    VALUES ({name}, {address}, {road_address}, {latitude}, {longitude}, {category}, {phone}, {description});",
        "END;",
        "",
    ])
lines.extend(["COMMIT TRANSACTION;", ""])
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
OUTPUT.write_text("\n".join(lines), encoding="utf-8-sig")
print(f"생성 완료: {OUTPUT.resolve()} ({len(rows)}개 자동 매칭 행, 중복은 건너뜀)")
