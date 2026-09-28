window.petMap = {
    init: function (places) {
        console.log("받은 장소 데이터:", places);

        const container = document.getElementById("map");

        const options = {
            center: new kakao.maps.LatLng(37.4751, 126.6432),
            level: 4
        };

        const map = new kakao.maps.Map(container, options);

        console.log("카카오 지도 생성 완료");

        // DB 장소를 Marker로 표시
        places.forEach(place => {

            const position = new kakao.maps.LatLng(
                place.latitude,
                place.longitude
            );

            const marker = new kakao.maps.Marker({
                position: position,
                map: map
            });

        });
    }
};