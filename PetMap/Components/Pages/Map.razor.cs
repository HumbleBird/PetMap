using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PetMap.Components.Pages.Models;
using PetMap.Data.Utiles;

namespace PetMap.Components.Pages
{
    public partial class Map
    {
        private const string kakaoAppKey = "957bb72f70345ef9382bb74c5cb8cfbe";
        private string currentUrl = "/map";

        protected override void OnInitialized()
        {
            currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
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
                    }
                );
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
