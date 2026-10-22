using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using PetMap.Data;

namespace PetMap.Components.Pages;

public partial class Landing : ComponentBase, IAsyncDisposable
{
    [Inject] private IWebHostEnvironment Environment { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ApplicationDbContext Db { get; set; } = default!;

    private LandingPageContent? content;
    private IJSObjectReference? effectsModule;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        effectsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/src/landingEffects.js");
        await effectsModule.InvokeVoidAsync("observeLandingSections");
    }

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

        await LoadRecommendedPlacesAsync();
    }

    private async Task LoadRecommendedPlacesAsync()
    {
        try
        {
            var ratingStats = await Db.PlaceRatings.AsNoTracking()
                .GroupBy(rating => rating.PlaceId)
                .Select(group => new { PlaceId = group.Key, Average = group.Average(rating => rating.Score), Count = group.Count() })
                .ToDictionaryAsync(item => item.PlaceId);
            var views = await Db.PlaceViews.AsNoTracking().GroupBy(view => view.PlaceId)
                .Select(group => new { PlaceId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.PlaceId, item => item.Count);
            var favorites = await Db.PlaceFavorites.AsNoTracking().GroupBy(item => item.PlaceId)
                .Select(group => new { PlaceId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.PlaceId, item => item.Count);
            var comments = await Db.PlaceComments.AsNoTracking().GroupBy(item => item.PlaceId)
                .Select(group => new { PlaceId = group.Key, Count = group.Count(), Likes = group.Sum(comment => comment.LikesCount) })
                .ToDictionaryAsync(item => item.PlaceId);

            var places = await Db.Places.AsNoTracking()
                .Select(place => new
                {
                    place.Id,
                    place.Name,
                    place.Address,
                    place.Category,
                    ImageUrl = Db.PlaceImages.Where(image => image.PlaceId == place.Id)
                        .OrderBy(image => image.Id)
                        .Select(image => image.FilePath)
                        .FirstOrDefault()
                })
                .ToListAsync();
            content!.SamplePlaces = places
                .Select(place =>
                {
                    ratingStats.TryGetValue(place.Id, out var rating);
                    views.TryGetValue(place.Id, out var viewCount);
                    favorites.TryGetValue(place.Id, out var favoriteCount);
                    comments.TryGetValue(place.Id, out var comment);
                    var reviewCount = rating?.Count ?? 0;
                    var commentCount = comment?.Count ?? 0;
                    var likes = comment?.Likes ?? 0;
                    // 평점 품질과 실제 이용 반응을 함께 반영하고, 로그 점수로 큰 수치 하나가 독점하지 않게 한다.
                    var score = (rating?.Average ?? 0) * 20
                        + Math.Log(1 + reviewCount) * 6
                        + Math.Log(1 + viewCount) * 5
                        + Math.Log(1 + favoriteCount) * 8
                        + Math.Log(1 + commentCount) * 8
                        + Math.Log(1 + likes) * 3;
                    var addressParts = place.Address.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var region = addressParts.Length >= 2 ? string.Join(' ', addressParts.Take(2)) : place.Address;
                    return new LandingPlacePreview
                    {
                        PlaceId = place.Id,
                        Name = place.Name,
                        Category = $"{place.Category} · {region}",
                        Description = $"★ {(rating?.Average ?? 0):0.0} · 리뷰 {reviewCount} · 조회 {viewCount} · 찜 {favoriteCount} · 댓글 좋아요 {likes}",
                        Icon = place.Category.Contains("카페") ? "☕" : place.Category.Contains("공원") ? "🌿" : "🐾",
                        ImageUrl = place.ImageUrl,
                        PopularityScore = score
                    };
                })
                .OrderByDescending(place => place.PopularityScore)
                .ThenBy(place => place.Name)
                .Take(3)
                .ToList();
        }
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
        {
            // DB 연결이 없는 개발 환경에서도 랜딩 페이지의 나머지 콘텐츠는 표시한다.
            content!.SamplePlaces = [];
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (effectsModule is null) return;

        try
        {
            await effectsModule.InvokeVoidAsync("stopLandingSectionObservation");
            await effectsModule.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Navigating away may close the Blazor circuit before module disposal.
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
        HeroDescription = "연성대학교 반려동물보건과",
        Features =
        [
            new() { Number = "01", Title = "프로젝트 소개", Description = "연성대학교 반려동물보건과 학생들이 기획한 ‘댕동여지도’는 안양시에서 반려동물과 함께 방문할 수 있는 장소를 쉽고 편리하게 찾을 수 있도록 돕는 서비스입니다.\n이를 통해 반려인들의 정보 접근성을 높이고 올바르고 책임감 있는 반려문화를 조성하고자 합니다.", Tags = ["동반 장소", "지역 정보", "편리한 탐색"] },
            new() { Number = "02", Title = "펫티켓", Description = "1. 목줄 착용 의무 (2M 이내)\n2. 과도한 짖음 통제\n3. 배변 즉시 처리 의무\n4. 낯선 반려견 간 인사 지양\n5. 비반려인 전용 공간 침범금지\n6. 정기 예방접종 완료\n", Tags = ["카페", "음식점", "공원", "동물병원"] },
            new() { Number = "03", Title = "반려동물 동반 출입 시 준비사항", Description = "-예방접종 증명서 준비\n-’반려동물 동반출입 음식점' 안내문 확인 \n-케이지 또는 목줄 필수", Tags = ["키링 태그", "웹사이트 연결"] }
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
    public int PlaceId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🐾";
    public string? ImageUrl { get; set; }
    public double PopularityScore { get; set; }
}
