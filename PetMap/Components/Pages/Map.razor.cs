using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using PetMap.Components.Pages.Models;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace PetMap.Components.Pages
{
    public partial class Map : IAsyncDisposable
    {
        private const string kakaoAppKey = "957bb72f70345ef9382bb74c5cb8cfbe";
        private string currentUrl = "/map";
        private const long MaxCommentImageSize = 5 * 1024 * 1024;
        private int? selectedPlaceId;
        private int? lastMiniMapPlaceId;
        private PlaceDetails? selectedPlace;
        private List<string> selectedPlaceImages = new();
        private List<CommentItem> placeComments = new();
        private DotNetObjectReference<Map>? _placeSelectCallback;
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
                        p.PetEntryCriteria,
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
                        appKey = kakaoAppKey,
                        mapId = "map",
                        level = 6
                    },
                    _placeSelectCallback
                );
            }

            if (selectedPlace is not null && lastMiniMapPlaceId != selectedPlace.Id && _mapModule is not null)
            {
                lastMiniMapPlaceId = selectedPlace.Id;
                await _mapModule.InvokeVoidAsync("showPlaceMiniMap", selectedPlace.Latitude, selectedPlace.Longitude, "place-mini-map", kakaoAppKey);
            }
        }

        [JSInvokable]
        public async Task OnMapPlaceSelected(int placeId)
        {
            var place = await Db.Places.SingleOrDefaultAsync(item => item.Id == placeId);
            if (place is null) return;

            selectedPlaceId = placeId;
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

            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
            isAuthenticated = !string.IsNullOrWhiteSpace(userId);
            currentAuthorName = auth.User.Identity?.Name ?? "회원";
            currentUserRating = isAuthenticated
                ? await Db.PlaceRatings.Where(rating => rating.PlaceId == placeId && rating.UserId == userId).Select(rating => rating.Score).FirstOrDefaultAsync()
                : 0;
            isFavorite = isAuthenticated && await Db.PlaceFavorites.AnyAsync(favorite => favorite.PlaceId == placeId && favorite.UserId == userId);
            await LoadCommentsAsync();
            commentError = "";
            // 평점/댓글을 불러온 결과를 열린 패널에 반영한다.
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadCommentsAsync()
        {
            if (selectedPlaceId is null) return;
            var userId = (await AuthStateProvider.GetAuthenticationStateAsync()).User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = Db.PlaceComments.Where(comment => comment.PlaceId == selectedPlaceId.Value);
            query = commentSort == "likes"
                ? query.OrderByDescending(comment => comment.LikesCount).ThenByDescending(comment => comment.CreatedAt)
                : query.OrderByDescending(comment => comment.CreatedAt);

            var rows = await query.Select(comment => new
            {
                comment.Id, comment.AuthorName, comment.UserId, comment.Content, comment.ImagePathsJson,
                comment.LikesCount, comment.CreatedAt,
                Rating = Db.PlaceRatings.Where(rating => rating.PlaceId == comment.PlaceId && rating.UserId == comment.UserId)
                    .Select(rating => (int?)rating.Score).FirstOrDefault(),
                HasLiked = userId != null && Db.PlaceCommentLikes.Any(like => like.CommentId == comment.Id && like.UserId == userId)
            }).ToListAsync();
            placeComments = rows.Select(row => new CommentItem
            {
                Id = row.Id, AuthorName = row.AuthorName, Content = row.Content, LikesCount = row.LikesCount, HasLiked = row.HasLiked,
                CreatedAt = row.CreatedAt, Rating = row.Rating,
                ImagePaths = string.IsNullOrWhiteSpace(row.ImagePathsJson)
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(row.ImagePathsJson) ?? new List<string>()
            }).ToList();
        }

        private async Task RateSelectedPlaceAsync(int score)
        {
            if (selectedPlaceId is null || score is < 1 or > 5) return;
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return;
            var rating = await Db.PlaceRatings.SingleOrDefaultAsync(item => item.PlaceId == selectedPlaceId && item.UserId == userId);
            if (rating is null)
                Db.PlaceRatings.Add(new PlaceRating { PlaceId = selectedPlaceId.Value, UserId = userId, Score = score, UpdatedAtUtc = DateTime.UtcNow });
            else { rating.Score = score; rating.UpdatedAtUtc = DateTime.UtcNow; }
            await Db.SaveChangesAsync();
            currentUserRating = score;
            var scores = await Db.PlaceRatings.Where(item => item.PlaceId == selectedPlaceId).Select(item => item.Score).ToListAsync();
            selectedPlace!.RatingCount = scores.Count;
            selectedPlace.AverageRating = scores.Count == 0 ? 0 : scores.Average();
            if (_mapModule is not null)
                await _mapModule.InvokeVoidAsync("updatePlaceRating", selectedPlaceId.Value, new { averageRating = selectedPlace.AverageRating, ratingCount = selectedPlace.RatingCount, currentUserRating = score });
        }

        private async Task ToggleFavoriteAsync()
        {
            if (selectedPlaceId is null) return;
            var userId = (await AuthStateProvider.GetAuthenticationStateAsync()).User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) { Navigation.NavigateTo("/Account/Login?ReturnUrl=%2Fmap"); return; }
            var favorite = await Db.PlaceFavorites.FindAsync(selectedPlaceId.Value, userId);
            if (favorite is null) Db.PlaceFavorites.Add(new PlaceFavorite { PlaceId = selectedPlaceId.Value, UserId = userId });
            else Db.PlaceFavorites.Remove(favorite);
            await Db.SaveChangesAsync();
            isFavorite = favorite is null;
        }

        private void HandleCommentImages(InputFileChangeEventArgs args)
        {
            if (args.FileCount > 5)
            {
                commentImages.Clear();
                commentError = "이미지는 최대 5장까지 추가할 수 있습니다.";
                return;
            }
            commentImages = args.GetMultipleFiles(5).ToList();
            commentError = "";
        }

        private async Task SubmitCommentAsync()
        {
            commentError = "";
            if (selectedPlaceId is null) return;
            if (string.IsNullOrWhiteSpace(commentText)) { commentError = "댓글 내용을 입력해주세요."; return; }
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var author = isAuthenticated ? (auth.User.Identity?.Name ?? "회원") : guestName.Trim();
            if (!isAuthenticated && (string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(guestPassword)))
            { commentError = "비회원 아이디와 비밀번호를 입력해주세요."; return; }

            var paths = new List<string>();
            var folder = Path.Combine(Environment.WebRootPath, "uploads", "comments");
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (commentImages.Any(file => file.Size > MaxCommentImageSize || !allowedExtensions.Contains(Path.GetExtension(file.Name))))
            {
                commentError = "이미지는 JPG, PNG, GIF, WEBP 형식으로 장당 5MB까지 첨부할 수 있습니다.";
                return;
            }
            Directory.CreateDirectory(folder);
            foreach (var file in commentImages.Take(5))
            {
                var extension = Path.GetExtension(file.Name);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                await using var stream = File.Create(Path.Combine(folder, fileName));
                await file.OpenReadStream(MaxCommentImageSize).CopyToAsync(stream);
                paths.Add($"/uploads/comments/{fileName}");
            }

            var comment = new PlaceComment
            {
                PlaceId = selectedPlaceId.Value,
                UserId = isAuthenticated ? userId : null,
                AuthorName = author,
                Content = commentText.Trim(),
                ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(paths)
            };
            if (!isAuthenticated)
                comment.GuestPasswordHash = new PasswordHasher<PlaceComment>().HashPassword(comment, guestPassword);
            Db.PlaceComments.Add(comment);
            await Db.SaveChangesAsync();
            commentText = ""; guestName = ""; guestPassword = ""; commentImages.Clear();
            await LoadCommentsAsync();
        }

        private async Task SortComments(string sort)
        {
            commentSort = sort;
            await LoadCommentsAsync();
        }

        private async Task LikeCommentAsync(int commentId)
        {
            var userId = (await AuthStateProvider.GetAuthenticationStateAsync()).User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                Navigation.NavigateTo("/Account/Login?ReturnUrl=%2Fmap");
                return;
            }

            var comment = await Db.PlaceComments.SingleOrDefaultAsync(item => item.Id == commentId && item.PlaceId == selectedPlaceId);
            if (comment is null) return;
            var existingLike = await Db.PlaceCommentLikes.FindAsync(commentId, userId);
            if (existingLike is null)
            {
                Db.PlaceCommentLikes.Add(new PlaceCommentLike { CommentId = commentId, UserId = userId });
                comment.LikesCount++;
            }
            else
            {
                Db.PlaceCommentLikes.Remove(existingLike);
                comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            }
            await Db.SaveChangesAsync();
            await LoadCommentsAsync();
        }

        private void ClosePlaceDetails()
        {
            selectedPlaceId = null; selectedPlace = null; lastMiniMapPlaceId = null;
        }

        private static string GetNaverSearchUrl(PlaceDetails place)
        {
            if (Uri.TryCreate(place.NaverPlaceUrl, UriKind.Absolute, out var savedUrl)
                && savedUrl.Host.EndsWith("map.naver.com", StringComparison.OrdinalIgnoreCase))
                return savedUrl.ToString();
            return $"https://map.naver.com/p/search/{Uri.EscapeDataString(place.Name)}";
        }

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
            public string? Description { get; set; }
            public string? Memo { get; set; }
            public string? BusinessHours { get; set; }
            public string? NaverPlaceUrl { get; set; }
            public DateTime? ReportDate { get; set; }
            public double AverageRating { get; set; }
            public int RatingCount { get; set; }
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

        // 모달 열림 여부
        private bool isFilterModalOpen;

        // 현재 선택된 탭
        private string activeCategoryGroupKey = "Leisure";

        // 저장된 필터 상태
        private HashSet<AnimalCategory> appliedCategories = new();

        // 모달 안에서 수정 중인 임시 필터 상태
        private HashSet<AnimalCategory> draftCategories = new();

        // UI 그룹 구성: 새 탭을 추가할 때 이 목록만 확장하면 된다.
        private readonly List<CategoryGroup> categoryGroups =
        [
            new CategoryGroup(
            "Leisure",
            "여가 & 문화",
            new[]
            {
                AnimalCategory.DogCafe,
                AnimalCategory.BakeryCafe,
                AnimalCategory.Restaurant,
                AnimalCategory.Accommodation,
                AnimalCategory.Studio,
                AnimalCategory.ShoppingAndCulture,
                AnimalCategory.Park
            }
        ),

        new CategoryGroup(
            "Care",
            "케어 & 돌봄",
            new[]
            {
                AnimalCategory.Hospital,
                AnimalCategory.GroomingShop,
                AnimalCategory.PetSupplyShop,
                AnimalCategory.DayCare,
                AnimalCategory.PetHotel,
                AnimalCategory.TrainingCenter,
                AnimalCategory.SelfWash
            }
        ),

        new CategoryGroup(
            "Service",
            "전문 서비스",
            new[]
            {
                AnimalCategory.PetTaxi,
                AnimalCategory.FuneralService
            }
        )
        ];

        // enum 값과 화면에 표시할 한국어 이름을 분리한다.
        private readonly Dictionary<AnimalCategory, string> categoryLabels =
            new()
            {
                [AnimalCategory.DogCafe] = "애견 전용 카페 / 운동장",
                [AnimalCategory.BakeryCafe] = "애견 동반 카페 / 베이커리",
                [AnimalCategory.Restaurant] = "애견 동반 식당",
                [AnimalCategory.Accommodation] = "애견 동반 숙소",
                [AnimalCategory.Studio] = "사진관 / 스튜디오",
                [AnimalCategory.ShoppingAndCulture] = "쇼핑 / 문화",
                [AnimalCategory.Park] = "공원",

                [AnimalCategory.Hospital] = "동물병원",
                [AnimalCategory.GroomingShop] = "애견 미용실",
                [AnimalCategory.PetSupplyShop] = "애견 용품점",
                [AnimalCategory.DayCare] = "유치원 / 데이케어",
                [AnimalCategory.PetHotel] = "애견 호텔",
                [AnimalCategory.TrainingCenter] = "훈련소 / 행동 교정",
                [AnimalCategory.SelfWash] = "셀프 목욕탕",

                [AnimalCategory.PetTaxi] = "펫택시",
                [AnimalCategory.FuneralService] = "장례 / 추모 시설"
            };

        private int TotalCategoryCount => Enum.GetValues<AnimalCategory>().Length;

        private IEnumerable<AnimalCategory> GetActiveCategories()
        {
            return categoryGroups
                .FirstOrDefault(g => g.Key == activeCategoryGroupKey)
                ?.Categories
                ?? Array.Empty<AnimalCategory>();
        }

        private string GetCategoryLabel(AnimalCategory category)
        {
            return categoryLabels.TryGetValue(category, out var label)
                ? label
                : category.ToString();
        }

        private void SelectCategoryGroup(string key)
        {
            activeCategoryGroupKey = key;
        }

        private void OpenFilter()
        {
            // 임시 선택 상태를 기존 적용 상태로 초기화한다.
            draftCategories = new HashSet<AnimalCategory>(
                appliedCategories
            );

            isFilterModalOpen = true;
        }

        private void ToggleCategory(
            AnimalCategory category,
            ChangeEventArgs e)
        {
            var isChecked = e.Value is bool value && value;

            if (isChecked)
            {
                draftCategories.Add(category);
            }
            else
            {
                draftCategories.Remove(category);
            }
        }

        private void ClearDraftFilter()
        {
            draftCategories.Clear();
        }

        private void CancelFilter()
        {
            // appliedCategories는 건드리지 않으므로 기존 적용 상태가 유지된다.
            isFilterModalOpen = false;
        }

        private string GetDraftFilterText()
        {
            if (draftCategories.Count == 0 ||
                draftCategories.Count == TotalCategoryCount)
            {
                return "전체 장소 표시";
            }

            return $"{draftCategories.Count}개 카테고리 선택";
        }

        private string GetFilterButtonText()
        {
            if (appliedCategories.Count == 0 ||
                appliedCategories.Count == TotalCategoryCount)
            {
                return "전체";
            }

            return $"{appliedCategories.Count}개 선택";
        }

        private async Task SaveFilter()
        {
            // 0개 선택 또는 전체 선택은 모두 '전체 표시'로 처리한다.
            var showAll =
                draftCategories.Count == 0 ||
                draftCategories.Count == TotalCategoryCount;

            appliedCategories = new HashSet<AnimalCategory>(
                draftCategories
            );

            isFilterModalOpen = false;

            if (_mapModule is null)
            {
                return;
            }

            var categories = appliedCategories
                .Select(category => category.ToString())
                .ToArray();

            await _mapModule.InvokeVoidAsync(
                "applyCategoryFilter",
                categories,
                showAll
            );
        }

        private IJSObjectReference? _mapModule;

        private string searchKeyword = "";
        private HashSet<AnimalCategory> selectedSearchCategories = new();
        private Dictionary<AnimalCategory, HashSet<string>> selectedSearchSubcategories = new();
        private List<KakaoSearchPlace> searchResults = new();
        private CancellationTokenSource? searchDebounce;

        private async Task SearchPlacesAsync()
        {
            CancelPendingSearch();
            searchResults.Clear();
            var keyword = searchKeyword.Trim();
            if (keyword.Length < 2 || _mapModule is null) return;

            using var debounce = new CancellationTokenSource();
            searchDebounce = debounce;
            var filters = BuildSearchFilters();
            var filterKey = GetSearchFilterKey();
            try
            {
                await Task.Delay(350, debounce.Token);
                var results = await _mapModule.InvokeAsync<List<KakaoSearchPlace>>(
                    "searchPlaces", keyword, filters);
                if (!debounce.IsCancellationRequested && keyword == searchKeyword.Trim()
                    && filterKey == GetSearchFilterKey())
                {
                    searchResults = results ?? new List<KakaoSearchPlace>();
                }
            }
            catch (OperationCanceledException) { }
            catch (JSException)
            {
                if (!debounce.IsCancellationRequested) searchResults.Clear();
            }
            finally
            {
                // 현재 요청이 아직 최신 요청일 때만 공유 참조를 비운다.
                if (ReferenceEquals(searchDebounce, debounce))
                {
                    searchDebounce = null;
                }
            }
        }

        private Task ToggleSearchCategory(AnimalCategory category)
        {
            if (!selectedSearchCategories.Add(category))
            {
                selectedSearchCategories.Remove(category);
                selectedSearchSubcategories.Remove(category);
            }
            return SearchPlacesAsync();
        }

        private Task ClearSearchCategories()
        {
            selectedSearchCategories.Clear();
            selectedSearchSubcategories.Clear();
            return SearchPlacesAsync();
        }

        private Task ToggleSearchSubtype(AnimalCategory category, string subtype)
        {
            if (!selectedSearchSubcategories.TryGetValue(category, out var selected))
            {
                selected = new HashSet<string>();
                selectedSearchSubcategories[category] = selected;
            }

            if (!selected.Add(subtype)) selected.Remove(subtype);
            if (selected.Count == 0) selectedSearchSubcategories.Remove(category);
            return SearchPlacesAsync();
        }

        private Task ClearSearchSubcategories(AnimalCategory category)
        {
            selectedSearchSubcategories.Remove(category);
            return SearchPlacesAsync();
        }

        private bool HasSelectedSubcategories(AnimalCategory category) =>
            selectedSearchSubcategories.TryGetValue(category, out var selected) && selected.Count > 0;

        private bool IsSearchSubtypeSelected(AnimalCategory category, string subtype) =>
            selectedSearchSubcategories.TryGetValue(category, out var selected) && selected.Contains(subtype);

        private List<SearchCategoryFilter> BuildSearchFilters() => selectedSearchCategories
            .Select(category => new SearchCategoryFilter
            {
                Category = category.ToString(),
                Subcategories = selectedSearchSubcategories.TryGetValue(category, out var selected)
                    ? selected.ToList()
                    : new List<string>()
            })
            .ToList();

        private string GetSearchFilterKey() => string.Join("|", Enum.GetValues<AnimalCategory>()
            .Where(selectedSearchCategories.Contains)
            .Select(category => $"{category}:{string.Join(",", selectedSearchSubcategories.TryGetValue(category, out var selected) ? selected.OrderBy(value => value) : Enumerable.Empty<string>())}"));

        private static string GetSearchCategoryLabel(AnimalCategory category) => category switch
        {
            AnimalCategory.DogCafe => "애견 카페/운동장",
            AnimalCategory.BakeryCafe => "동반 카페/베이커리",
            AnimalCategory.Restaurant => "레스토랑",
            AnimalCategory.Accommodation => "숙소",
            AnimalCategory.Studio => "사진관/스튜디오",
            AnimalCategory.ShoppingAndCulture => "쇼핑/문화",
            AnimalCategory.Park => "공원",
            AnimalCategory.Hospital => "동물병원",
            AnimalCategory.GroomingShop => "미용실",
            AnimalCategory.PetSupplyShop => "반려동물 용품점",
            AnimalCategory.DayCare => "유치원/데이케어",
            AnimalCategory.PetHotel => "애견 호텔",
            AnimalCategory.TrainingCenter => "훈련소",
            AnimalCategory.SelfWash => "셀프 목욕",
            AnimalCategory.PetTaxi => "펫택시",
            AnimalCategory.FuneralService => "장례/추모 시설",
            _ => category.ToString()
        };

        private static string[] GetSearchSubcategories(AnimalCategory category) => category switch
        {
            AnimalCategory.Restaurant => ["한식", "일식", "중식", "양식", "분식", "주점"],
            AnimalCategory.Accommodation => ["펜션", "호텔", "글램핑"],
            AnimalCategory.ShoppingAndCulture => ["쇼핑몰", "전시회", "아울렛"],
            _ => Array.Empty<string>()
        };

        private void ClearSearch()
        {
            searchKeyword = "";
            searchResults.Clear();
            CancelPendingSearch();
        }

        private void CancelPendingSearch()
        {
            var pendingSearch = searchDebounce;
            searchDebounce = null;

            try
            {
                pendingSearch?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // 요청이 끝나는 순간 토큰이 해제됐으면 취소할 작업이 없다.
            }
        }

        private async Task FocusPlace(KakaoSearchPlace place)
        {
            searchKeyword = place.Name;
            searchResults.Clear();
            if (_mapModule is not null)
            {
                await _mapModule.InvokeVoidAsync("moveToPlace", place);
            }
        }

        private sealed class KakaoSearchPlace
        {
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string RoadAddress { get; set; } = "";
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Category { get; set; } = "";
            public string Phone { get; set; } = "";
            public string PlaceUrl { get; set; } = "";
        }

        private sealed class SearchCategoryFilter
        {
            public string Category { get; set; } = "";
            public List<string> Subcategories { get; set; } = new();
        }
    }
}
