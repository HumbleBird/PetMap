import { loadKakao } from '../loaders/kakaoLoader.js';
import { getMarkerColor, markerImageByColor } from '../markers/markerFactory.js';
import { createThumbnailOverlay } from './thumbnailOverlay.js';
import { createRatingOverlay } from './ratingOverlay.js';

const THUMBNAIL_LEVEL = 4;
const ANYANG_UNIVERSITY_CENTER = { latitude: 37.3911593, longitude: 126.9197963 };

// 지도와 현재 장소 목록은 필터링 및 확대 수준 변경에 재사용한다.
let currentMap = null;
let placeItems = [];
let externalSearchMarker = null;

// Kakao 지도를 만들고 장소 마커, 정보창, 썸네일을 연결한다.
export async function initMap(places = [], options = {}, placeSelectedCallback = null) {
    if (!options.appKey) {
        throw new Error('Kakao appKey 필요: options.appKey');
    }

    const kakao = await loadKakao(options.appKey, { autoload: false });
    const mapElement = document.getElementById(options.mapId || 'map');

    if (!mapElement) {
        throw new Error(`지도를 표시할 요소를 찾을 수 없습니다: ${options.mapId || 'map'}`);
    }

    const map = new kakao.maps.Map(mapElement, {
        center: new kakao.maps.LatLng(
            ANYANG_UNIVERSITY_CENTER.latitude,
            ANYANG_UNIVERSITY_CENTER.longitude
        ),
        level: options.level ?? 5
    });

    currentMap = map;
    // 각 장소의 마커와 썸네일을 만들어 관리 목록에 보관한다.
    placeItems = (places ?? []).map(place => createPlaceItem(kakao, map, place, placeSelectedCallback));

    // 안양시 만안구와 동안구 행정경계를 지도에 표시한다.
    await drawAnyangDistricts(kakao, map);

    // 확대 수준이 바뀌면 썸네일 표시 여부를 다시 계산한다.
    const refreshThumbnails = () => updateThumbnails(map);
    kakao.maps.event.addListener(map, 'zoom_changed', refreshThumbnails);
    refreshThumbnails();

    return { map };
}

async function drawAnyangDistricts(kakao, map) {
    try {
        const response = await fetch('/data/anyang-districts.geojson');
        if (!response.ok) throw new Error(`행정경계 파일 로드 실패: ${response.status}`);
        const geoJson = await response.json();
        const districtStyles = {
            '안양시 만안구': { stroke: '#2864c5', fill: '#4f8df5' },
            '안양시 동안구': { stroke: '#e67822', fill: '#f6a04d' }
        };

        geoJson.features.forEach(feature => {
            const districtName = feature.properties.name;
            const style = districtStyles[districtName];
            if (!style) return;

            const polygonRings = feature.geometry.type === 'Polygon'
                ? feature.geometry.coordinates
                : feature.geometry.coordinates.flat();
            const outerRing = polygonRings[0] ?? [];
            const path = outerRing.map(([longitude, latitude]) =>
                new kakao.maps.LatLng(latitude, longitude));

            new kakao.maps.Polygon({
                map,
                path,
                strokeWeight: 3,
                strokeColor: style.stroke,
                strokeOpacity: 0.9,
                strokeStyle: 'solid',
                fillColor: style.fill,
                fillOpacity: 0.08,
                zIndex: 1
            });

            const center = getRingCenter(outerRing);
            new kakao.maps.CustomOverlay({
                map,
                position: new kakao.maps.LatLng(center.latitude, center.longitude),
                content: `<div class="district-map-label" style="--district-color:${style.stroke}">${districtName.replace('안양시 ', '')}</div>`,
                yAnchor: 0.5,
                zIndex: 2
            });
        });
    } catch (error) {
        console.warn('안양시 구 경계를 표시하지 못했습니다.', error);
    }
}

// GeoJSON 외곽선의 면적 중심을 계산해 구 이름을 경계 안에 배치한다.
function getRingCenter(ring) {
    let areaTwice = 0;
    let longitudeTotal = 0;
    let latitudeTotal = 0;

    for (let index = 0; index < ring.length - 1; index++) {
        const [longitudeA, latitudeA] = ring[index];
        const [longitudeB, latitudeB] = ring[index + 1];
        const cross = longitudeA * latitudeB - longitudeB * latitudeA;
        areaTwice += cross;
        longitudeTotal += (longitudeA + longitudeB) * cross;
        latitudeTotal += (latitudeA + latitudeB) * cross;
    }

    if (Math.abs(areaTwice) < 1e-12) {
        const points = ring.slice(0, -1);
        return {
            longitude: points.reduce((sum, point) => sum + point[0], 0) / points.length,
            latitude: points.reduce((sum, point) => sum + point[1], 0) / points.length
        };
    }

    return {
        longitude: longitudeTotal / (3 * areaTwice),
        latitude: latitudeTotal / (3 * areaTwice)
    };
}

// 장소 하나에 마커, 클릭 이벤트, 썸네일 오버레이를 연결한다.
function createPlaceItem(kakao, map, place, placeSelectedCallback) {
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
    const ratingOverlay = createRatingOverlay(kakao, place, position);

    marker.setMap(map);
    if (place.ratingCount > 0) ratingOverlay.setMap(map);
    const item = {
        marker,
        place,
        overlay: createThumbnailOverlay(kakao, place, position),
        ratingOverlay,
        placeName: place.name ?? '',
        categories,
        visible: true
    };

    kakao.maps.event.addListener(marker, 'click', () => {
        if (!placeSelectedCallback) {
            console.error('장소 상세 패널 콜백이 연결되지 않았습니다.');
            return;
        }
        placeSelectedCallback.invokeMethodAsync('OnMapPlaceSelected', Number(place.id))
            .catch(error => console.error('장소 상세 정보를 열지 못했습니다.', error));
    });

    return item;
}

// 상세 패널을 열어 지도 폭이 바뀐 뒤 기존 지도 타일을 다시 배치한다.
export function relayoutMap() {
    if (currentMap) currentMap.relayout();
}

// 상세 패널에서 별점을 저장한 뒤 마커 배지에도 새 평균을 갱신한다.
export function updatePlaceRating(placeId, summary) {
    const item = placeItems.find(entry => Number(entry.place.id) === Number(placeId));
    if (!item) return;
    item.ratingOverlay.update(summary);
    item.ratingOverlay.setMap(item.visible && summary.ratingCount > 0 ? currentMap : null);
}

// 랜딩의 추천 장소 링크를 열면 해당 DB 장소의 마커를 지도 중앙으로 이동한다.
export function focusMapPlace(placeId) {
    if (!currentMap) return;
    const item = placeItems.find(entry => Number(entry.place.id) === Number(placeId));
    if (!item) return;

    const position = new window.kakao.maps.LatLng(item.place.latitude, item.place.longitude);
    currentMap.setLevel(4);
    currentMap.panTo(position);
}

// 상세 패널 안에 선택한 장소만 표시하는 작은 위치 지도를 만든다.
export async function renderPlaceMiniMap(latitude, longitude, elementId, appKey) {
    const kakao = await loadKakao(appKey, { autoload: false });
    const element = document.getElementById(elementId);
    if (!element) return;
    const position = new kakao.maps.LatLng(latitude, longitude);
    const miniMap = new kakao.maps.Map(element, {
        center: position,
        level: 3,
        draggable: false,
        scrollwheel: false,
        disableDoubleClick: true
    });
    new kakao.maps.Marker({ map: miniMap, position });
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
        item.ratingOverlay?.setMap(item.visible && item.place.ratingCount > 0 ? currentMap : null);
    });
}

// 여러 선택 조건을 각각 카카오에 검색하고 중복 장소를 합친다.
export async function searchKakaoPlaces(keyword, filters = []) {
    if (!window.kakao?.maps?.services) {
        return Promise.reject(new Error('카카오 장소 검색 서비스를 사용할 수 없습니다.'));
    }

    const requests = filters.length === 0
        ? [searchKakaoCategory(keyword, null, null)]
        : filters.flatMap(filter => {
            const subcategories = filter.subcategories?.length ? filter.subcategories : [null];
            return subcategories.map(subcategory =>
                searchKakaoCategory(keyword, filter.category, subcategory));
        });

    const resultSets = await Promise.all(requests);
    const uniquePlaces = new Map();
    resultSets.flat().forEach(place => {
        const key = place.placeUrl || `${place.name}:${place.latitude}:${place.longitude}`;
        if (!uniquePlaces.has(key)) uniquePlaces.set(key, place);
    });
    return [...uniquePlaces.values()].slice(0, 15);
}

// Kakao가 제공하는 대분류 코드와 세부 종류 검색어를 함께 사용한다.
function searchKakaoCategory(keyword, category, subtype) {
    const service = new window.kakao.maps.services.Places();
    const categoryGroupCodeByType = {
        DogCafe: 'CE7',
        BakeryCafe: 'CE7',
        Restaurant: 'FD6',
        Accommodation: 'AD5',
        PetHotel: 'AD5',
        Park: 'AT4',
        Hospital: 'HP8'
    };
    const categoryGroupCode = category === 'ShoppingAndCulture'
        ? (subtype === '전시회' ? 'CT1' : subtype ? 'MT1' : undefined)
        : categoryGroupCodeByType[category];
    const searchHints = {
        DogCafe: '애견 카페 운동장',
        BakeryCafe: '애견 동반 카페 베이커리',
        Restaurant: '음식점',
        Accommodation: '숙소',
        Studio: '사진관 스튜디오',
        ShoppingAndCulture: '쇼핑 문화시설',
        Park: '공원',
        Hospital: '동물병원',
        GroomingShop: '애견 미용실',
        PetSupplyShop: '애견용품 반려동물 용품점',
        DayCare: '애견 유치원 데이케어',
        PetHotel: '애견 호텔',
        TrainingCenter: '반려견 훈련소 행동 교정',
        SelfWash: '애견 셀프 목욕',
        PetTaxi: '펫택시',
        FuneralService: '반려동물 장례 추모'
    };
    const needsSpecificKeyword = ['DogCafe', 'BakeryCafe', 'Hospital', 'PetHotel'].includes(category);
    const queryPrefix = subtype ?? ((categoryGroupCode && !needsSpecificKeyword)
        ? ''
        : searchHints[category] ?? '');
    const query = queryPrefix ? `${queryPrefix} ${keyword}` : keyword;
    return new Promise((resolve, reject) => {
        service.keywordSearch(query, (data, status) => {
            if (status === window.kakao.maps.services.Status.OK) {
                let results = data.map(place => ({
                    name: place.place_name,
                    address: place.address_name,
                    roadAddress: place.road_address_name,
                    latitude: Number(place.y),
                    longitude: Number(place.x),
                    category: place.category_name,
                    phone: place.phone,
                    placeUrl: place.place_url
                }));

                resolve(results);
            } else if (status === window.kakao.maps.services.Status.ZERO_RESULT) {
                resolve([]);
            } else {
                reject(new Error('카카오 장소 검색에 실패했습니다.'));
            }
        }, { size: 15, ...(categoryGroupCode ? { category_group_code: categoryGroupCode } : {}) });
    });
}

// 위임 이벤트를 사용해 뒤늦게 렌더링된 하위 카테고리에도 휠 이동을 적용한다.
let searchWheelBound = false;
export function enableSearchCategoryWheel() {
    if (searchWheelBound) return;
    searchWheelBound = true;
    document.addEventListener('wheel', event => {
        const element = event.target.closest?.('.map-search-categories, .map-search-subcategories');
        if (!element || element.scrollWidth <= element.clientWidth) return;
        const delta = Math.abs(event.deltaY) > Math.abs(event.deltaX) ? event.deltaY : event.deltaX;
        if (delta === 0) return;
        element.scrollLeft += delta;
        event.preventDefault();
    }, { passive: false });
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
