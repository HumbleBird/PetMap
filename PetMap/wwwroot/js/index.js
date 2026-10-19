
console.log('[index.js] 파일 로드 시작');

import {
    initMap,
    filterCategories
} from './src/map/MapManager.js';

export async function init(places, options = {}) {
    return await initMap(places, options);
}

export function applyCategoryFilter(categories, showAll) {
    filterCategories(categories, showAll);
}

