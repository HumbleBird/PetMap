using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using PetMap.Components.Pages.Models;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;
using PetMap.Components.Pages.Main;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace PetMap.Components.Pages
{
    public partial class Map : IAsyncDisposable
    {
        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private string KakaoJavaScriptKey => Configuration["Kakao:JavaScriptKey"] ?? string.Empty;
        private string currentUrl = "/map";
        private const long MaxCommentImageSize = 5 * 1024 * 1024;
        private int? selectedPlaceId;
        private int? lastMiniMapPlaceId;
        private PlaceDetails? selectedPlace;
        private List<string> selectedPlaceImages = new();
        private List<CommentItem> placeComments = new();
        private DotNetObjectReference<Map>? _placeSelectCallback;
        private Report? reportComponent;
        private bool isPlaceMenuOpen;
        private bool showTagEditor;
        private bool showBusinessHoursEditor;
        private bool showPlaceHistoryModal;
        private bool showPlaceReportModal;
        private bool isMapSearchMinimized;
        private bool isSavedPlacesPanelOpen;
        private string newPlaceTags = "";
        private string newBusinessHours = "";
        private string newPhone = "";
        private string phoneContributionMessage = "";
        private string placeReportReason = "";
        private string placeReportMessage = "";
        private string placeContributionMessage = "";
        private string shareMessage = "";
        private List<IBrowserFile> placeImageFiles = new();
        private List<PlaceReport> selectedPlaceHistory = new();
        private List<string> selectedPlaceTags = new();
        private List<SavedPlaceCard> savedPlaces = new();
        private string savedPlacesMessage = "";

        [SupplyParameterFromQuery(Name = "placeId")]
        public int? SharedPlaceId { get; set; }
        private bool isAuthenticated;
        private bool isFavorite;
        private int currentUserRating;
        private string currentAuthorName = "";
        private string guestName = "";
        private string guestPassword = "";
        private string commentText = "";
        private string commentSort = "latest";
        private string commentError = "";
        private List<IBrowserFile> commentImages = new();

        protected override void OnInitialized()
        {
            currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var auth = await AuthStateProvider.GetAuthenticationStateAsync();
                isAuthenticated = auth.User.Identity?.IsAuthenticated == true;
                currentAuthorName = auth.User.Identity?.Name ?? "회원";
                var currentUserId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
                _placeSelectCallback = DotNetObjectReference.Create(this);

                // DB에서 장소 가져오기
                var places = await Db.Places
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Address,
                        p.Latitude,
                        p.Longitude,
                        p.Description,
                        p.Category,
                        p.Phone,
                        p.BusinessHours,
                        p.RestaurantType,
                        AverageRating = Db.PlaceRatings
                            .Where(rating => rating.PlaceId == p.Id)
                            .Select(rating => (double?)rating.Score)
                            .Average(),
                        RatingCount = Db.PlaceRatings.Count(rating => rating.PlaceId == p.Id),
                        CurrentUserRating = Db.PlaceRatings
                            .Where(rating => rating.PlaceId == p.Id && rating.UserId == currentUserId)
                            .Select(rating => (int?)rating.Score)
                            .FirstOrDefault(),

                        // 해당 장소의 대표 이미지 경로
                        ImageUrl = Db.PlaceImages
                            .Where(img => img.PlaceId == p.Id)
                            .Select(img => img.FilePath)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                // 2. JS 모듈 import
                _mapModule = await JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./js/index.js"
                );

                // 3. 실제 지도 초기화
                await _mapModule.InvokeVoidAsync(
                    "init",
                    places,
                    new
                    {
                        appKey = KakaoJavaScriptKey,
                        mapId = "map",
                        level = 6
                    },
                    _placeSelectCallback
                );
                if (!string.IsNullOrWhiteSpace(currentUserId))
                    await LoadSavedPlacesAsync(currentUserId);

                // 공유 링크에 장소 ID가 있으면 지도가 열린 뒤 해당 장소 패널도 바로 연다.
                if (SharedPlaceId is int sharedPlaceId)
                {
                    await _mapModule.InvokeVoidAsync("focusPlace", sharedPlaceId);
                    await OnMapPlaceSelected(sharedPlaceId);
                }
            }

            if (selectedPlace is not null && lastMiniMapPlaceId != selectedPlace.Id && _mapModule is not null)
            {
                lastMiniMapPlaceId = selectedPlace.Id;
                await _mapModule.InvokeVoidAsync("showPlaceMiniMap", selectedPlace.Latitude, selectedPlace.Longitude, "place-mini-map", KakaoJavaScriptKey);
            }
        }

        [JSInvokable]
        public async Task OnMapPlaceSelected(int placeId)
        {
            var place = await Db.Places.SingleOrDefaultAsync(item => item.Id == placeId);
            if (place is null) return;

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var viewerId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Db.PlaceViews.Add(new PlaceView { PlaceId = placeId, UserId = viewerId, ViewedAtUtc = DateTime.UtcNow });
            await Db.SaveChangesAsync();

            selectedPlaceId = placeId;
            isPlaceMenuOpen = false;
            showTagEditor = false;
            showBusinessHoursEditor = false;
            newPlaceTags = "";
            newBusinessHours = "";
            newPhone = place.Phone ?? "";
            phoneContributionMessage = "";
            placeContributionMessage = "";
            shareMessage = "";
            placeImageFiles.Clear();
            selectedPlace = new PlaceDetails
            {
                Id = place.Id,
                Name = place.Name,
                Address = place.Address,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                Category = place.Category,
                RestaurantType = place.RestaurantType,
                Menu = place.Menu,
                Phone = place.Phone,
                Description = place.Description,
                Memo = place.Memo,
                BusinessHours = place.BusinessHours,
                NaverPlaceUrl = place.NaverPlaceUrl
            };

            // 상세 패널은 로그인 상태나 부가 정보 조회에 의존하지 않도록 먼저 연다.
            selectedPlaceImages = new();
            placeComments = new();
            currentUserRating = 0;
            isFavorite = false;
            await InvokeAsync(StateHasChanged);

            // 평점, 제보일, 사진처럼 추가로 조회하는 정보는 패널을 연 뒤 채운다.
            selectedPlace.AverageRating = await Db.PlaceRatings
                .Where(rating => rating.PlaceId == placeId)
                .Select(rating => (double?)rating.Score)
                .AverageAsync() ?? 0;
            selectedPlace.RatingCount = await Db.PlaceRatings.CountAsync(rating => rating.PlaceId == placeId);
            selectedPlace.ReportDate = await Db.PlaceReports
                .Where(report => report.PlaceName == place.Name)
                .OrderBy(report => report.CreatedAt)
                .Select(report => (DateTime?)report.CreatedAt)
                .FirstOrDefaultAsync();
            selectedPlaceImages = await Db.PlaceImages.Where(image => image.PlaceId == placeId)
                .Select(image => image.FilePath).ToListAsync();
            selectedPlaceTags = await Db.PlaceReports
                .Where(report => report.PlaceName == place.Name
                    && report.Review == "[태그 추가]")
                .Select(report => report.Memo)
                .Where(tags => tags != null)
                .Select(tags => tags!)
                .ToListAsync();
            selectedPlaceTags = selectedPlaceTags
                .SelectMany(tags => (tags ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
            isAuthenticated = !string.IsNullOrWhiteSpace(userId);
            currentAuthorName = auth.User.Identity?.Name ?? "회원";
            currentUserRating = isAuthenticated
                ? await Db.PlaceRatings.Where(rating => rating.PlaceId == placeId && rating.UserId == userId).Select(rating => rating.Score).FirstOrDefaultAsync()
                : 0;
            isFavorite = isAuthenticated && await Db.PlaceFavorites.AnyAsync(favorite => favorite.PlaceId == placeId && favorite.UserId == userId);
            if (isAuthenticated && userId is not null)
                await LoadSavedPlacesAsync(userId);
            await LoadCommentsAsync();
            commentError = "";
            // 평점/댓글을 불러온 결과를 열린 패널에 반영한다.
            await InvokeAsync(StateHasChanged);
        }

        private bool HasPlaceTags => selectedPlace is not null
            && selectedPlaceTags.Count > 0;

        private void TogglePlaceMenu() => isPlaceMenuOpen = !isPlaceMenuOpen;

        private sealed class PlaceDetails
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Category { get; set; } = "";
            public string? RestaurantType { get; set; }
            public string? Menu { get; set; }
            public string? Phone { get; set; }
            public string? Description { get; set; }
            public string? Memo { get; set; }
            public string? BusinessHours { get; set; }
            public string? NaverPlaceUrl { get; set; }
            public DateTime? ReportDate { get; set; }
            public double AverageRating { get; set; }
            public int RatingCount { get; set; }
        }

        private sealed class SavedPlaceCard
        {
            public int PlaceId { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Category { get; set; } = "";
            public string? ImageUrl { get; set; }
            public string? FolderName { get; set; }
            public string? Memo { get; set; }
        }

        private sealed class CommentItem
        {
            public int Id { get; set; }
            public string AuthorName { get; set; } = "";
            public string Content { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int? Rating { get; set; }
            public int LikesCount { get; set; }
            public bool HasLiked { get; set; }
            public List<string> ImagePaths { get; set; } = new();
        }

        public async ValueTask DisposeAsync()
        {
            _placeSelectCallback?.Dispose();
            if (_mapModule is not null)
            {
                try
                {
                    await _mapModule.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // 로그아웃/페이지 종료로 회로가 끊기면 브라우저 모듈은 해제할 수 없다.
                }
            }
        }

    }
}
