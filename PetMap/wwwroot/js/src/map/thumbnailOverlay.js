// 대표 이미지가 있는 장소에 지도 위 썸네일 오버레이를 만든다.
export function createThumbnailOverlay(kakao, place, position) {
    if (!place.imageUrl) return null;

    const card = document.createElement('div');
    Object.assign(card.style, {
        width: '76px',
        height: '60px',
        padding: '3px',
        boxSizing: 'border-box',
        background: '#fff',
        border: '1px solid #ddd',
        borderRadius: '7px',
        boxShadow: '0 2px 7px rgba(0,0,0,.2)',
        overflow: 'hidden',
        transform: 'translate(12px, -34px)'
    });

    const image = document.createElement('img');
    image.src = place.imageUrl;
    image.alt = place.name || '장소 대표 이미지';
    Object.assign(image.style, {
        width: '100%',
        height: '100%',
        display: 'block',
        objectFit: 'cover',
        borderRadius: '4px'
    });
    card.appendChild(image);

    return new kakao.maps.CustomOverlay({
        position,
        content: card,
        xAnchor: 0,
        yAnchor: 1,
        zIndex: 3
    });
}
