using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;

namespace PetMap.Components.Pages;

public partial class Landing : ComponentBase
{
    [Inject] private IWebHostEnvironment Environment { get; set; } = default!;

    private LandingPageContent? content;

    protected override async Task OnInitializedAsync()
    {
        var contentPath = Path.Combine(Environment.WebRootPath, "data", "landing-page.json");
        if (!File.Exists(contentPath))
        {
            content = LandingPageContent.CreatePlaceholder();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(contentPath);
            content = JsonSerializer.Deserialize<LandingPageContent>(
                json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? LandingPageContent.CreatePlaceholder();
        }
        catch (JsonException)
        {
            content = LandingPageContent.CreatePlaceholder();
        }
    }
}

public sealed class LandingPageContent
{
    public string BrandName { get; set; } = "";
    public string BrandSubtitle { get; set; } = "";
    public string HeroTitle { get; set; } = "";
    public string HeroLead { get; set; } = "";
    public string HeroDescription { get; set; } = "";
    public string MananLabel { get; set; } = "만안구";
    public string DonganLabel { get; set; } = "동안구";
    public List<LandingFeature> Features { get; set; } = [];
    public string GuideTitle { get; set; } = "";
    public string GuideDescription { get; set; } = "";
    public string RulesTitle { get; set; } = "";
    public string RulesDescription { get; set; } = "";
    public List<string> Rules { get; set; } = [];
    public string PlacesTitle { get; set; } = "";
    public List<LandingPlacePreview> SamplePlaces { get; set; } = [];
    public string MobileCardTitle { get; set; } = "";
    public string MobileCardDescription { get; set; } = "";
    public string FooterMessage { get; set; } = "";

    public static LandingPageContent CreatePlaceholder() => new()
    {
        BrandName = "땡동여지도",
        BrandSubtitle = "안양시 반려동물 동반 장소 지도 서비스",
        HeroTitle = "반려동물과 더 가까운 안양",
        HeroLead = "함께 갈 수 있는 장소를 한눈에",
        HeroDescription = "이 문구는 관리자 콘텐츠로 교체할 수 있습니다. 서비스의 특징과 이용 방법을 소개해 주세요.",
        Features =
        [
            new() { Number = "01", Title = "프로젝트 소개", Description = "프로젝트 설명이 들어가는 영역입니다.", Tags = ["동반 장소", "지역 정보", "편리한 탐색"] },
            new() { Number = "02", Title = "펫플레이스 정보 제공", Description = "장소 정보와 카테고리를 소개하는 영역입니다.", Tags = ["카페", "음식점", "공원", "동물병원"] },
            new() { Number = "03", Title = "NFC 키링 연동", Description = "NFC 키링과 지도 서비스를 설명하는 영역입니다.", Tags = ["키링 태그", "웹사이트 연결"] },
            new() { Number = "04", Title = "반려동물 생활 안내", Description = "반려동물과 함께 이용할 때 필요한 안내를 보여주는 영역입니다.", Tags = ["안전", "펫티켓"] }
        ],
        GuideTitle = "반려동물과 함께하는 안양 생활",
        GuideDescription = "안내와 추천 장소를 소개하는 콘텐츠 영역입니다.",
        RulesTitle = "함께 지키는 약속",
        RulesDescription = "산책과 장소 방문 전에 확인할 안내를 작성해 주세요.",
        Rules = ["목줄과 인식표를 착용해 주세요.", "방문 장소의 동반 기준을 확인해 주세요.", "배변 봉투를 준비해 주세요."],
        PlacesTitle = "오늘의 추천 장소",
        SamplePlaces =
        [
            new() { Name = "추천 장소 이름", Category = "공원 · 동안구", Description = "장소 한 줄 소개", Icon = "🌿" },
            new() { Name = "추천 장소 이름", Category = "카페 · 만안구", Description = "장소 한 줄 소개", Icon = "☕" },
            new() { Name = "추천 장소 이름", Category = "음식점 · 안양시", Description = "장소 한 줄 소개", Icon = "🍽️" }
        ],
        MobileCardTitle = "추천 장소 이름",
        MobileCardDescription = "장소 설명이 표시되는 영역입니다.",
        FooterMessage = "함께 걷는 안양, 반려동물과 더 가까운 일상"
    };
}

public sealed class LandingFeature
{
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}

public sealed class LandingPlacePreview
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🐾";
}
