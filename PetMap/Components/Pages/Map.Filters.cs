using Microsoft.AspNetCore.Components;
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

}
