const REPORT_DRAFT_KEY = 'petmap.reportDraft';

// 제보 입력값을 현재 브라우저의 로컬 저장소에 임시 보관한다.
export function saveReportDraft(json) {
    localStorage.setItem(REPORT_DRAFT_KEY, json);
}

// 저장된 제보 초안을 읽는다. 초안이 없으면 null을 반환한다.
export function loadReportDraft() {
    return localStorage.getItem(REPORT_DRAFT_KEY);
}

// 사용자가 저장하지 않기로 선택했거나 제보를 제출한 경우 초안을 지운다.
export function clearReportDraft() {
    localStorage.removeItem(REPORT_DRAFT_KEY);
}
