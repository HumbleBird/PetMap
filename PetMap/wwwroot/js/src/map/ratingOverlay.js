// 별점이 있는 장소의 평균 점수를 마커 오른쪽 아래에 표시한다.
export function createRatingOverlay(kakao, place, position) {
    const badge = document.createElement('div');
    Object.assign(badge.style, {
        padding: '3px 7px',
        border: '1px solid #f0d59d',
        borderRadius: '999px',
        background: 'rgba(255, 255, 255, .97)',
        color: '#805313',
        boxShadow: '0 2px 7px rgba(35, 42, 51, .2)',
        font: '700 11px/1.2 sans-serif',
        whiteSpace: 'nowrap',
        pointerEvents: 'none',
        transform: 'translate(13px, -3px)'
    });

    const overlay = new kakao.maps.CustomOverlay({
        position,
        content: badge,
        xAnchor: 0,
        yAnchor: 1,
        zIndex: 4
    });

    const update = summary => {
        place.averageRating = summary.averageRating;
        place.ratingCount = summary.ratingCount;
        place.currentUserRating = summary.currentUserRating;
        badge.textContent = `★ ${Number(summary.averageRating).toFixed(1)}`;
    };

    if (place.ratingCount > 0) update(place);

    return {
        overlay,
        update,
        setMap(map) {
            overlay.setMap(map);
        }
    };
}
