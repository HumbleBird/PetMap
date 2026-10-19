
const cache = new Map();

// 카테고리별 마커 색상
const categoryColorMap = {
    // 여가 & 문화
    DogCafe: '#E74C3C',
    BakeryCafe: '#F39C12',
    Restaurant: '#D35400',
    Accommodation: '#8E44AD',
    Studio: '#E84393',
    ShoppingAndCulture: '#6C5CE7',

    // 케어 & 돌봄
    Hospital: '#2980B9',
    GroomingShop: '#16A085',
    PetSupplyShop: '#27AE60',
    DayCare: '#2ECC71',
    PetHotel: '#3498DB',
    TrainingCenter: '#1ABC9C',
    SelfWash: '#00B4D8',

    // 전문 서비스
    PetTaxi: '#34495E',
    FuneralService: '#7F8C8D'
};

// 카테고리에 맞는 색상 반환
export function getMarkerColor(category) {
    return categoryColorMap[category] ?? '#7F8C8D';
}

// 색상에 맞는 카카오맵 마커 이미지 생성
export function markerImageByColor(
    kakao,
    color,
    { width = 36, height = 48 } = {}
) {
    const cacheKey = `${color}_${width}_${height}`;

    if (cache.has(cacheKey)) {
        return cache.get(cacheKey);
    }

    const svg = `
        <svg xmlns="http://www.w3.org/2000/svg"
             width="${width}" height="${height}"
             viewBox="0 0 36 48">
            <path
                d="M18 0C8.6 0 1 7.6 1 17
                   C1 28 18 48 18 48
                   S35 28 35 17
                   C35 7.6 27.4 0 18 0Z"
                fill="${color}"
                stroke="#FFFFFF"
                stroke-width="1.5"
            />
            <circle cx="18" cy="17" r="6" fill="#FFFFFF"/>
        </svg>
    `;

    const url =
        'data:image/svg+xml;charset=UTF-8;base64,' +
        btoa(svg);

    const image = new kakao.maps.MarkerImage(
        url,
        new kakao.maps.Size(width, height),
        {
            offset: new kakao.maps.Point(width / 2, height)
        }
    );

    cache.set(cacheKey, image);

    return image;
}
