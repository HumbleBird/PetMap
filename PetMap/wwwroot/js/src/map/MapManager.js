import { loadKakao } from '../loaders/kakaoLoader.js';
import { getMarkerColor, markerImageByColor } from '../markers/markerFactory.js';
import { createPlaceInfoWindow } from './placeInfoWindow.js';
import { createThumbnailOverlay } from './thumbnailOverlay.js';

const THUMBNAIL_LEVEL = 4;

// 지도와 현재 장소 목록은 필터링 및 확대 수준 변경에 재사용한다.
let currentMap = null;
let placeItems = [];

// Kakao 지도를 만들고 장소 마커, 정보창, 썸네일을 연결한다.
export async function initMap(places = [], options = {}) {
    if (!options.appKey) {
        throw new Error('Kakao appKey 필요: options.appKey');
    }

    const kakao = await loadKakao(options.appKey, { autoload: false });
    const mapElement = document.getElementById(options.mapId || 'map');

    if (!mapElement) {
        throw new Error(`지도를 표시할 요소를 찾을 수 없습니다: ${options.mapId || 'map'}`);
    }

    const map = new kakao.maps.Map(mapElement, {
        center: new kakao.maps.LatLng(37.4751, 126.6432),
        level: options.level ?? 5
    });

    const infoWindow = createPlaceInfoWindow(kakao);
    currentMap = map;
    // 각 장소의 마커와 썸네일을 만들어 관리 목록에 보관한다.
    placeItems = (places ?? []).map(place => createPlaceItem(kakao, map, place, infoWindow));

    // 확대 수준이 바뀌면 썸네일 표시 여부를 다시 계산한다.
    const refreshThumbnails = () => updateThumbnails(map);
    kakao.maps.event.addListener(map, 'zoom_changed', refreshThumbnails);
    refreshThumbnails();

    return { map };
}

// 장소 하나에 마커, 클릭 이벤트, 썸네일 오버레이를 연결한다.
function createPlaceItem(kakao, map, place, infoWindow) {
    const position = new kakao.maps.LatLng(place.latitude, place.longitude);
    // 장소는 여러 카테고리를 가질 수 있으므로 마커 색상은 첫 카테고리로 정한다.
    const categories = String(place.category ?? '')
        .split(',')
        .map(category => category.trim())
        .filter(Boolean);
    const marker = new kakao.maps.Marker({
        position,
        image: markerImageByColor(kakao, getMarkerColor(categories[0])),
        title: place.name ?? ''
    });

    marker.setMap(map);
    kakao.maps.event.addListener(marker, 'click', () => infoWindow.open(map, marker, place));

    return {
        marker,
        overlay: createThumbnailOverlay(kakao, place, position),
        categories,
        visible: true
    };
}

// 확대 수준과 필터 상태에 따라 썸네일을 표시하거나 숨긴다.
function updateThumbnails(map) {
    const shouldShow = map.getLevel() <= THUMBNAIL_LEVEL;

    placeItems.forEach(item => {
        item.overlay?.setMap(shouldShow && item.visible ? map : null);
    });
}

// 선택한 카테고리에 맞춰 마커와 썸네일을 표시하거나 숨긴다.
export function filterCategories(categories = [], showAll = false) {
    if (!currentMap) {
        console.warn('지도가 초기화되지 않았습니다.');
        return;
    }

    const selectedCategories = new Set(categories);
    const showThumbnails = currentMap.getLevel() <= THUMBNAIL_LEVEL;

    placeItems.forEach(item => {
        item.visible = showAll || item.categories.some(category => selectedCategories.has(category));
        item.marker.setMap(item.visible ? currentMap : null);
        item.overlay?.setMap(item.visible && showThumbnails ? currentMap : null);
    });
}
