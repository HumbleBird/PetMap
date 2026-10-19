
console.log('[index.js] 파일 로드 시작');

// index.js
import { initMap } from './src/map/MapManager.js';


window.petMap = {
  // Blazor: JS.InvokeVoidAsync("petMap.init", places, { appKey: '...' })
    init: function(places, options = {}) {
        initMap(places, options).catch(err => {
          console.error('petMap.init 실패:', err);
    });
  }
};

