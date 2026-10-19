// Legacy bridge used by Report.razor for Kakao Places keyword search.
// Map rendering and marker interactions live in js/src/map/.
window.petMap = {
    // 검색 객체는 첫 검색 때 한 번 만들고 이후 검색에서 재사용한다.
    placeService: null,

    // Kakao 키워드 검색 결과를 C#의 ReceiveSearchResults 메서드로 전달한다.
    searchPlaces(keyword, dotNetRef) {
        if (!this.placeService) {
            this.placeService = new kakao.maps.services.Places();
        }

        this.placeService.keywordSearch(
            keyword,
            (data, status) => {
                // 성공한 경우 최대 5개 결과를 앱에서 사용하는 필드로 변환한다.
                const results = status === kakao.maps.services.Status.OK
                    ? data.slice(0, 5).map(place => ({
                        Name: place.place_name,
                        RoadAddress: place.road_address_name,
                        JibunAddress: place.address_name,
                        PlaceUrl: place.place_url,
                        BusinessHours: '',
                        Latitude: Number(place.y),
                        Longitude: Number(place.x)
                    }))
                    : [];

                dotNetRef.invokeMethodAsync('ReceiveSearchResults', results);
            },
            { size: 5 }
        );
    }
};
