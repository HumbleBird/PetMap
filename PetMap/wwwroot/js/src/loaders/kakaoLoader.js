// kakaoLoader.js
export function loadKakao(appKey, { version = '2.1.0', autoload = false } = {}) {
  return new Promise((resolve, reject) => {
    if (typeof window.kakao !== 'undefined') {
      // 이미 로드된 경우
      if (window.kakao.maps && typeof window.kakao.maps.load === 'function') {
        window.kakao.maps.load(() => resolve(window.kakao));
      } else {
        resolve(window.kakao);
      }
      return;
    }

    const src = `https://dapi.kakao.com/v2/maps/sdk.js?appkey=${encodeURIComponent(appKey)}&libraries=services&autoload=${autoload ? 'true' : 'false'}&version=${version}`;
    const s = document.createElement('script');
    s.src = src;
    s.async = true;
    s.onerror = (e) => reject(new Error('Kakao SDK 로드 실패'));
    s.onload = () => {
      if (!autoload && window.kakao && typeof window.kakao.maps !== 'undefined' && typeof window.kakao.maps.load === 'function') {
        window.kakao.maps.load(() => resolve(window.kakao));
      } else if (window.kakao) {
        resolve(window.kakao);
      } else {
        reject(new Error('kakao 객체가 존재하지 않음'));
      }
    };
    document.head.appendChild(s);
  });
}