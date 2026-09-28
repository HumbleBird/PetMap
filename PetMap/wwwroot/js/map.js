window.petMap = {
    init: function () {
        console.log("petMap.init 실행됨");

        const container = document.getElementById("map");

        if (!container) {
            console.error("map 요소를 찾을 수 없습니다.");
            return;
        }

        const options = {
            center: new kakao.maps.LatLng(37.4751, 126.6432),
            level: 4
        };

        const map = new kakao.maps.Map(container, options);

        console.log("카카오 지도 생성 완료");
    }
};