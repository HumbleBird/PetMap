window.petMap = {
    map: null,
    markers: [],
    infoWindow: null,

    init: function (places) {
        console.log("받은 장소 데이터:", places);

        const container = document.getElementById("map");

        const options = {
            center: new kakao.maps.LatLng(37.4751, 126.6432),
            level: 4
        };

        const map = new kakao.maps.Map(container, options);

        this.map = map;

        console.log("카카오 지도 생성 완료");

        this.createMarkers(places);
    },

    createMarkers: function (places) {

        // 기존 Marker 제거
        this.markers.forEach(marker => {
            marker.marker.setMap(null);
        });

        this.markers = [];

        const map = this.map;

        places.forEach(place => {

            // 위치 갱신
            const position = new kakao.maps.LatLng(
                place.latitude,
                place.longitude
            );

            // 마커 갱신
            const marker = new kakao.maps.Marker({
                position: position,
                map: this.map
            });

            // 장소 정보
            const infoContent = `
                <div style="
                    padding: 12px;
                    width: 220px;
                    font-size: 14px;
                ">
                    <strong>${place.name}</strong>

                    <div style="
                        margin-top: 5px;
                        color: #555;
                    ">
                        ${place.category}
                    </div>

                    <hr style="margin: 8px 0;">

                    <div>
                        ${place.address}
                    </div>

                    <div style="margin-top: 5px;">
                        ${place.description ?? ""}
                    </div>
                </div>
            `;

            // Marker 클릭
            kakao.maps.event.addListener(marker, "click", function () {

                console.log("마커 클릭:", place.name);

                const content = document.createElement("div");

                content.style.cssText = `
                    padding: 15px;
                    background: white;
                    border: 1px solid #333;
                    width: 200px;
                `;

                content.innerHTML = `
                    <strong>${place.name}</strong>
                `;

                const infoWindow = new kakao.maps.InfoWindow({
                    content: infoContent
                });

                infoWindow.open(map, marker);
            });

            this.markers.push({
                marker: marker,
                category: place.category
            });
        });


        console.log("마커 생성 완료:", this.markers.length);
    },


    filterCategory: function (category) {

        console.log("선택한 카테고리:", category);

        this.markers.forEach(item => {

            if (category === "전체" ||
                item.category === category) {

                item.marker.setMap(this.map);

            } else {

                item.marker.setMap(null);

            }
        });
    }
};