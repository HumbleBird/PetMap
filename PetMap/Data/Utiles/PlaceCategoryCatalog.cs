namespace PetMap.Data.Utiles;

public sealed record PlaceCategoryGroup(string Key, string Title, IReadOnlyList<AnimalCategory> Categories);

public static class PlaceCategoryCatalog
{
    public static IReadOnlyList<PlaceCategoryGroup> Groups { get; } =
    [
        new("Leisure", "여가 & 문화",
        [
            AnimalCategory.DogCafe, AnimalCategory.BakeryCafe, AnimalCategory.Restaurant,
            AnimalCategory.Accommodation, AnimalCategory.Studio, AnimalCategory.ShoppingAndCulture,
            AnimalCategory.Park, AnimalCategory.LeisureOther
        ]),
        new("Care", "케어 & 돌봄",
        [
            AnimalCategory.Hospital, AnimalCategory.GroomingShop, AnimalCategory.PetSupplyShop,
            AnimalCategory.DayCare, AnimalCategory.PetHotel, AnimalCategory.TrainingCenter,
            AnimalCategory.SelfWash, AnimalCategory.CareOther
        ]),
        new("Service", "전문 서비스",
        [AnimalCategory.PetTaxi, AnimalCategory.FuneralService, AnimalCategory.SpecializedServiceOther])
    ];

    public static string GetLabel(AnimalCategory category) => category switch
    {
        AnimalCategory.DogCafe => "애견 전용 카페 / 운동장",
        AnimalCategory.BakeryCafe => "애견 동반 카페 / 베이커리",
        AnimalCategory.Restaurant => "애견 동반 식당",
        AnimalCategory.Accommodation => "애견 동반 숙소",
        AnimalCategory.Studio => "사진관 / 스튜디오",
        AnimalCategory.ShoppingAndCulture => "쇼핑 / 문화",
        AnimalCategory.Park => "공원",
        AnimalCategory.Hospital => "동물병원",
        AnimalCategory.GroomingShop => "애견 미용실",
        AnimalCategory.PetSupplyShop => "애견 용품점",
        AnimalCategory.DayCare => "유치원 / 데이케어",
        AnimalCategory.PetHotel => "애견 호텔",
        AnimalCategory.TrainingCenter => "훈련소 / 행동 교정",
        AnimalCategory.SelfWash => "셀프 목욕탕",
        AnimalCategory.PetTaxi => "펫택시",
        AnimalCategory.FuneralService => "장례 / 추모 시설",
        AnimalCategory.LeisureOther or AnimalCategory.CareOther or AnimalCategory.SpecializedServiceOther => "기타",
        _ => category.ToString()
    };

    public static string GetFilterLabel(AnimalCategory category) => category switch
    {
        AnimalCategory.LeisureOther => "여가 & 문화 기타",
        AnimalCategory.CareOther => "케어 & 돌봄 기타",
        AnimalCategory.SpecializedServiceOther => "전문 서비스 기타",
        _ => GetLabel(category)
    };

    public static string Normalize(string category) => category switch
    {
        "동물병원" => nameof(AnimalCategory.Hospital),
        "용품점" => nameof(AnimalCategory.PetSupplyShop),
        "공원" => nameof(AnimalCategory.Park),
        "미용" => nameof(AnimalCategory.GroomingShop),
        _ when Enum.TryParse<AnimalCategory>(category, ignoreCase: true, out var parsed) => parsed.ToString(),
        _ => category
    };
}
