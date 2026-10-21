
console.log('[index.js] 파일 로드 시작');

import {
    initMap,
    filterCategories,
    searchKakaoPlaces,
    moveToKakaoPlace,
    enableSearchCategoryWheel,
    relayoutMap,
    renderPlaceMiniMap,
    updatePlaceRating as updateMapPlaceRating
} from './src/map/MapManager.js';

export async function init(places, options = {}, placeSelectedCallback = null) {
    const result = await initMap(places, options, placeSelectedCallback);
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

export function relayout() {
    relayoutMap();
}

export function showPlaceMiniMap(latitude, longitude, elementId, appKey) {
    return renderPlaceMiniMap(latitude, longitude, elementId, appKey);
}

export function updatePlaceRating(placeId, summary) {
    updateMapPlaceRating(placeId, summary);
}

export async function sharePlace(title, text, url) {
    if (navigator.share) {
        await navigator.share({ title, text, url });
        return;
    }

    await navigator.clipboard.writeText(url);
}

