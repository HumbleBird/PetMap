
console.log('[index.js] 파일 로드 시작');

import {
    initMap,
    filterCategories,
    searchKakaoPlaces,
    moveToKakaoPlace
} from './src/map/MapManager.js';

export async function init(places, options = {}) {
    return await initMap(places, options);
}

export function applyCategoryFilter(categories, showAll) {
    filterCategories(categories, showAll);
}

export function searchPlaces(keyword) {
    return searchKakaoPlaces(keyword);
}

export function moveToPlace(place) {
    moveToKakaoPlace(place);
}

