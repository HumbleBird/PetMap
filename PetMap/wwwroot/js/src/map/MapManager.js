import { loadKakao } from '../loaders/kakaoLoader.js';
import { getMarkerColor, markerImageByColor } from '../markers/markerFactory.js';
import { createPlaceInfoWindow } from './placeInfoWindow.js';
import { createThumbnailOverlay } from './thumbnailOverlay.js';

const THUMBNAIL_LEVEL = 4;

// 지도와 현재 장소 목록은 필터링 및 확대 수준 변경에 재사용한다.
let currentMap = null;
let placeItems = [];
let externalSearchMarker = null;

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
        placeName: place.name ?? '',
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

// 카카오 로컬 장소 검색 API로 지도 주변 장소를 검색한다.
export function searchKakaoPlaces(keyword) {
    if (!window.kakao?.maps?.services) {
        return Promise.reject(new Error('카카오 장소 검색 서비스를 사용할 수 없습니다.'));
    }

    const service = new window.kakao.maps.services.Places();
    return new Promise((resolve, reject) => {
        service.keywordSearch(keyword, (data, status) => {
            if (status === window.kakao.maps.services.Status.OK) {
                resolve(data.map(place => ({
                    name: place.place_name,
                    address: place.address_name,
                    roadAddress: place.road_address_name,
                    latitude: Number(place.y),
                    longitude: Number(place.x),
                    category: place.category_name,
                    phone: place.phone,
                    placeUrl: place.place_url
                })));
            } else if (status === window.kakao.maps.services.Status.ZERO_RESULT) {
                resolve([]);
            } else {
                reject(new Error('카카오 장소 검색에 실패했습니다.'));
            }
        }, { size: 6 });
    });
}

// 카카오 검색 결과를 선택하면 별도 검색 마커를 놓고 해당 위치로 이동한다.
export function moveToKakaoPlace(place) {
    if (!currentMap || !place) return;

    const kakao = window.kakao;
    const position = new kakao.maps.LatLng(place.latitude, place.longitude);
    currentMap.setLevel(3);
    currentMap.panTo(position);

    if (externalSearchMarker) externalSearchMarker.setMap(null);
    externalSearchMarker = new kakao.maps.Marker({
        map: currentMap,
        position,
        title: place.name
    });
}
