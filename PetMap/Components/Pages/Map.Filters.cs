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
        private IReadOnlyList<PlaceCategoryGroup> categoryGroups = PlaceCategoryCatalog.Groups;

        private int TotalCategoryCount => Enum.GetValues<AnimalCategory>().Length;

        private IEnumerable<AnimalCategory> GetActiveCategories()
        {
            return categoryGroups
                .FirstOrDefault(g => g.Key == activeCategoryGroupKey)
                ?.Categories
                ?? Array.Empty<AnimalCategory>();
        }

        private static string GetCategoryLabel(AnimalCategory category) => PlaceCategoryCatalog.GetLabel(category);

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
