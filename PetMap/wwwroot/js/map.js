window.petMap = {
    map: null,
    markers: [],
    infoWindow: null,

    init2: function (places) {
        //console.log("받은 장소 데이터:", places);

        const container = document.getElementById("map");

        const options = {
            center: new kakao.maps.LatLng(37.4751, 126.6432),
            level: 4
        };

        const map = new kakao.maps.Map(container, options);

        this.map = map;

        //console.log("카카오 지도 생성 완료");

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
            // 업체가 등록한 전체 카테고리
            const categories = this.parseCategories(place.category);

            // 첫 번째 카테고리가 마커 색상을 결정
            const primaryCategory = categories[0] ?? "기타";

            // 첫 번째 카테고리의 아이콘
            const iconUrl = this.getCategoryIcon(primaryCategory);

            const markerImage = new kakao.maps.MarkerImage(
                iconUrl,
                new kakao.maps.Size(36, 36),
                {
                    offset: new kakao.maps.Point(18, 36)
                }
            );


            // 첫 번째 카테고리의 색상으로 마커 생성
            const color = this.getCategoryColor(primaryCategory);
            //wconst markerImage = this.createMarkerImage(color);

            const marker = new kakao.maps.Marker({
                position: position,
                map: this.map,
                image: markerImage
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
                category: place.category,
                categories: categories
            });
        });


        console.log("마커 생성 완료:", this.markers.length);
    },


    filterCategory: function (category) {

        console.log("선택한 카테고리:", category);

        this.markers.forEach(item => {
            const visible =
                category === "전체" ||
                item.categories.includes(category);

            item.marker.setMap(visible ? this.map : null);
        });
    },

    enablePlaceRegistration: function () {

        console.log("지도 장소 등록 모드");

        kakao.maps.event.addListener(
            this.map,
            "click",
            function (mouseEvent) {

                const latlng = mouseEvent.latLng;

                const latitude = latlng.getLat();
                const longitude = latlng.getLng();

                console.log("선택한 위치:", latitude, longitude);

                window.location.href =
                    `/places/create?lat=${latitude}&lng=${longitude}`;
            }
        );

        alert("장소를 등록할 위치를 지도에서 클릭하세요.");
    },

    searchPlaces: function (keyword, dotNetRef) {

        if (!this.placeService) {
            this.placeService = new kakao.maps.services.Places();
        }

        this.placeService.keywordSearch(
            keyword,
            function (data, status) {

                if (status === kakao.maps.services.Status.OK) {

                    const results = data.slice(0, 5).map(place => ({
                        Name: place.place_name,
                        RoadAddress: place.road_address_name,
                        JibunAddress: place.address_name,
                        PlaceUrl: place.place_url,
                        BusinessHours: "",
                        Latitude: Number(place.y),
                        Longitude: Number(place.x)
                    }));

                    dotNetRef.invokeMethodAsync(
                        "ReceiveSearchResults",
                        results
                    );
                }
                else {
                    dotNetRef.invokeMethodAsync(
                        "ReceiveSearchResults",
                        []
                    );
                }
            },
            {
                size: 5
            }
        );
    },

    // 카테고리 문자열을 배열로 변환
    parseCategories: function (category) {
        if (Array.isArray(category)) {
            return category.map(String).map(c => c.trim()).filter(Boolean);
        }

        if (!category) return [];

        // "동물병원, 용품점" 같은 문자열 처리
        return String(category)
            .split(/[,;|]/)
            .map(c => c.trim())
            .filter(Boolean);
    },

    // 카테고리별 색상 지정
    getCategoryColor: function (category) {
        const c = String(category ?? "").toLowerCase();

        if (c.includes("병원") || c.includes("hospital")) {
            return "#EF4444"; // 빨강
        }

        if (c.includes("용품") || c.includes("supplies")) {
            return "#3B82F6"; // 파랑
        }

        if (c.includes("공원") || c.includes("park")) {
            return "#22C55E"; // 초록
        }

        if (c.includes("동반") || c.includes("friendly")) {
            return "#F59E0B"; // 주황
        }

        return "#64748B"; // 기타: 회색
    },

    // 색상에 맞는 커스텀 마커 이미지 생성
    createMarkerImage: function (color) {
        const svg = `
        <svg xmlns="http://www.w3.org/2000/svg"
             width="36" height="45" viewBox="0 0 36 45">
            <path d="M18 1 C8.6 1 1 8.6 1 18
                     C1 29 18 44 18 44
                     C18 44 35 29 35 18
                     C35 8.6 27.4 1 18 1Z"
                  fill="${color}" stroke="#FFFFFF" stroke-width="2"/>
            <circle cx="18" cy="18" r="6" fill="#FFFFFF"/>
        </svg>
    `;

        const src =
            "data:image/svg+xml;charset=utf-8," +
            encodeURIComponent(svg);

        return new kakao.maps.MarkerImage(
            src,
            new kakao.maps.Size(36, 45),
            { offset: new kakao.maps.Point(18, 45) }
        );
    },

    getCategoryIcon: function (category) {
        return `/images/markers/${category}.png`;
    },
};