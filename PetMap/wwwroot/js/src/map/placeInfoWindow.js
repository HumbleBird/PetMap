// 마커를 눌렀을 때 장소 정보를 보여줄 Kakao 정보창을 만든다.
export function createPlaceInfoWindow(kakao) {
    const infoWindow = new kakao.maps.InfoWindow({ removable: true });

    return {
        open(map, marker, place) {
            infoWindow.setContent(createContent(place));
            infoWindow.open(map, marker);
        }
    };
}

// 장소 데이터를 안전하게 DOM 요소로 구성해 정보창 내용을 만든다.
function createContent(place) {
    const card = document.createElement('article');
    Object.assign(card.style, {
        width: '250px',
        maxHeight: '320px',
        overflowY: 'auto',
        padding: '12px',
        boxSizing: 'border-box',
        font: '14px/1.45 sans-serif',
        color: '#252a34'
    });

    if (place.imageUrl) {
        const image = document.createElement('img');
        image.src = place.imageUrl;
        image.alt = `${place.name || '장소'} 대표 이미지`;
        Object.assign(image.style, {
            width: '100%',
            height: '140px',
            objectFit: 'cover',
            borderRadius: '6px',
            marginBottom: '8px'
        });
        card.appendChild(image);
    }

    const title = document.createElement('strong');
    title.textContent = place.name || '이름 미등록';
    title.style.fontSize = '16px';
    card.appendChild(title);

    appendRow(card, '분류', place.restaurantType || place.category);
    appendRow(card, '주소', place.address);
    appendRow(card, '전화', place.phone);
    appendRow(card, '영업시간', place.businessHours);
    appendRow(card, '반려동물 출입 기준', place.petEntryCriteria || place.petPolicy);
    appendRow(card, '안내', place.description);
    if (place.ratingCount > 0) {
        appendRow(card, '평균 별점', `★ ${Number(place.averageRating).toFixed(1)} (${place.ratingCount}명)`);
    }

    return card;
}

// 값이 비어 있지 않을 때만 '항목: 값' 행을 정보창에 추가한다.
function appendRow(parent, label, value) {
    if (value === null || value === undefined || String(value).trim() === '') return;

    const row = document.createElement('div');
    row.style.marginTop = '6px';

    const heading = document.createElement('strong');
    heading.textContent = `${label}: `;
    row.appendChild(heading);
    row.appendChild(document.createTextNode(String(value)));
    parent.appendChild(row);
}
