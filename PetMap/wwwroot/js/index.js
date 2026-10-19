
console.log('[index.js] 파일 로드 시작');

import {
    initMap,
    filterCategories,
    searchKakaoPlaces,
    moveToKakaoPlace,
    enableSearchCategoryWheel
} from './src/map/MapManager.js';

export async function init(places, options = {}) {
    const result = await initMap(places, options);
    enableSearchCategoryWheel();
    return result;
}

export function applyCategoryFilter(categories, showAll) {
    filterCategories(categories, showAll);
}

export function searchPlaces(keyword, filters) {
    return searchKakaoPlaces(keyword, filters);
}

export function moveToPlace(place) {
    moveToKakaoPlace(place);
}

