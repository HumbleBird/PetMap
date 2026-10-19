
import { loadKakao } from '../loaders/kakaoLoader.js';
import {
    getMarkerColor,
    markerImageByColor
} from '../markers/markerFactory.js';

const THUMBNAIL_LEVEL = 4;


export async function initMap(places, options = {}) {
    const appKey = options.appKey;

    if (!appKey) {
        throw new Error('Kakao appKey 필요: options.appKey');
    }

    const kakao = await loadKakao(appKey, { autoload: false });

    const mapEl = document.getElementById(options.mapId || 'map');

    if (!mapEl) {
        throw new Error('지도 요소를 찾을 수 없습니다.');
    }

    const map = new kakao.maps.Map(mapEl, {
        center: new kakao.maps.LatLng(37.4751, 126.6432),
        level: options.level ?? 5
    });

    const placeItems = [];

    places.forEach(p => {
        const position = new kakao.maps.LatLng(
            p.latitude,
            p.longitude
        );

        // DB에서 받은 카테고리
        const category = p.category;

        // 카테고리에 맞는 색상 선택
        const color = getMarkerColor(category);

        // 해당 색상의 핀 이미지 생성
        const markerImage = markerImageByColor(kakao, color);

        // 마커 생성
        const marker = new kakao.maps.Marker({
            position,
            image: markerImage,
            title: p.name
        });

        marker.setMap(map);

        let overlay = null;

        // 2. 대표 이미지가 있는 장소만 썸네일 오버레이 생성
        if (p.imageUrl) {
            const card = document.createElement('div');

            Object.assign(card.style, {
                width: '76px',
                height: '60px',
                padding: '3px',
                boxSizing: 'border-box',
                background: '#fff',
                border: '1px solid #ddd',
                borderRadius: '7px',
                boxShadow: '0 2px 7px rgba(0,0,0,0.2)',
                overflow: 'hidden',
                transform: 'translate(12px, -34px)'
            });

            const img = document.createElement('img');
            img.src = p.imageUrl;
            img.alt = p.name || '장소 대표 이미지';

            Object.assign(img.style, {
                width: '100%',
                height: '100%',
                display: 'block',
                objectFit: 'cover',
                borderRadius: '4px'
            });

            card.appendChild(img);

            overlay = new kakao.maps.CustomOverlay({
                position,
                content: card,
                xAnchor: 0,
                yAnchor: 1,
                zIndex: 3
            });
        }

        placeItems.push({ marker, overlay });
    });

    // 3. 확대 수준에 따라 썸네일 표시 여부 변경
    function updateThumbnails() {
        // 카카오맵은 level 숫자가 작을수록 확대된 상태
        const shouldShow = map.getLevel() <= THUMBNAIL_LEVEL;

        placeItems.forEach(({ overlay }) => {
            if (!overlay) return;

            overlay.setMap(shouldShow ? map : null);
        });
    }

    kakao.maps.event.addListener(
        map,
        'zoom_changed',
        updateThumbnails
    );

    // 지도 최초 표시 시에도 현재 확대 수준 반영
    updateThumbnails();

    return { map };
}
