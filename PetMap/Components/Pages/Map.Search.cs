using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PetMap.Components.Pages.Models;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;
using System.Security.Claims;

namespace PetMap.Components.Pages;

public partial class Map
{
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

        private static string GetSearchCategoryLabel(AnimalCategory category) => PlaceCategoryCatalog.GetLabel(category);

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
