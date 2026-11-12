using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PetMap.Components.Pages.Models;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;
using PetMap.Services;
using System.Security.Claims;

namespace PetMap.Components.Pages;

public partial class Map
{
        private async Task OpenPlaceHistoryAsync()
        {
            isPlaceMenuOpen = false;
            if (selectedPlace is null) return;
            selectedPlaceHistory = await Db.PlaceReports
                .Where(report => report.PlaceName == selectedPlace.Name)
                .OrderByDescending(report => report.CreatedAt)
                .ToListAsync();
            showPlaceHistoryModal = true;
        }

        private static string GetReportStatusLabel(Status status) => status switch
        {
            Status.Pending => "검토 중",
            Status.Approved => "승인됨",
            Status.Rejected => "반려됨",
            _ => status.ToString()
        };

        private async Task OpenPlaceEditSuggestionAsync()
        {
            isPlaceMenuOpen = false;
            if (selectedPlace is null) return;
            if (reportComponent is null)
            {
                placeContributionMessage = "제보 창을 준비하는 중입니다. 잠시 후 다시 시도해 주세요.";
                return;
            }
            await reportComponent.OpenEditSuggestionAsync(selectedPlace.Id);
        }

        private async Task OpenMapReportAsync()
        {
            if (reportComponent is not null)
                await reportComponent.OpenReportFromMapAsync();
        }

        private void OpenPlaceReport()
        {
            isPlaceMenuOpen = false;
            placeReportReason = "";
            placeReportMessage = "";
            showPlaceReportModal = true;
        }

        private async Task SubmitPlaceReportAsync()
        {
            if (selectedPlace is null) return;
            if (string.IsNullOrWhiteSpace(placeReportReason))
            {
                placeReportMessage = "신고 내용을 입력해 주세요.";
                return;
            }

            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            Db.PlaceReports.Add(new PlaceReport
            {
                Nickname = user.Identity?.IsAuthenticated == true ? user.Identity.Name ?? "회원" : "방문자",
                PlaceName = selectedPlace.Name,
                Category = selectedPlace.Category,
                Latitude = selectedPlace.Latitude,
                Longitude = selectedPlace.Longitude,
                NaverPlaceUrl = selectedPlace.NaverPlaceUrl,
                BusinessHours = selectedPlace.BusinessHours,
                Review = $"[장소 신고] {placeReportReason.Trim()}",
                Status = Status.Pending
            });
            await Db.SaveChangesAsync();
            showPlaceReportModal = false;
        }

        private async Task ShareSelectedPlaceAsync()
        {
            isPlaceMenuOpen = false;
            if (selectedPlace is null || _mapModule is null) return;
            var shareUrl = Navigation.ToAbsoluteUri($"/map?placeId={selectedPlace.Id}").ToString();
            try
            {
                await _mapModule.InvokeVoidAsync("sharePlace", selectedPlace.Name, selectedPlace.Address, shareUrl);
                shareMessage = "장소 링크를 공유했어요.";
            }
            catch (JSException ex) when (ex.Message.Contains("AbortError", StringComparison.OrdinalIgnoreCase))
            {
                shareMessage = "공유를 취소했습니다.";
            }
            catch (JSException)
            {
                shareMessage = "공유 기능을 사용할 수 없습니다.";
            }
        }

        private async Task SavePlaceTagsAsync()
        {
            if (selectedPlace is null) return;
            var additions = newPlaceTags.Split([',', '，', ';', '；'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.StartsWith('#') ? tag : $"#{tag}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (additions.Length == 0 || additions.Any(tag => tag.Length > 40))
            {
                placeContributionMessage = "태그를 입력해 주세요. 태그는 40자까지 입력할 수 있어요.";
                return;
            }

            var place = await Db.Places.FindAsync(selectedPlace.Id);
            if (place is null) return;
            // 별도 태그 테이블이 아직 없어 기존 제보 테이블을 태그 저장소로 사용한다.
            Db.PlaceReports.Add(new PlaceReport
            {
                Nickname = "방문자",
                PlaceName = place.Name,
                Category = place.Category,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                NaverPlaceUrl = place.NaverPlaceUrl,
                BusinessHours = place.BusinessHours,
                Review = "[태그 추가]",
                Memo = string.Join(", ", additions),
                Status = Status.Approved
            });
            await Db.SaveChangesAsync();
            selectedPlaceTags = selectedPlaceTags
                .Concat(additions)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            newPlaceTags = "";
            showTagEditor = false;
            placeContributionMessage = "태그를 추가했습니다.";
        }

        private async Task SaveBusinessHoursAsync()
        {
            if (selectedPlace is null) return;
            if (string.IsNullOrWhiteSpace(newBusinessHours) || newBusinessHours.Length > 500)
            {
                placeContributionMessage = "영업시간을 입력해 주세요. 최대 500자까지 가능합니다.";
                return;
            }
            var place = await Db.Places.FindAsync(selectedPlace.Id);
            if (place is null) return;
            place.BusinessHours = newBusinessHours.Trim();
            await Db.SaveChangesAsync();
            selectedPlace.BusinessHours = place.BusinessHours;
            newBusinessHours = "";
            showBusinessHoursEditor = false;
            placeContributionMessage = "영업시간을 추가했습니다.";
        }

        private async Task SavePlacePhoneAsync()
        {
            if (selectedPlace is null) return;
            if (string.IsNullOrWhiteSpace(newPhone) || newPhone.Trim().Length > 40)
            {
                phoneContributionMessage = "전화번호를 입력해 주세요. 최대 40자까지 가능합니다.";
                return;
            }

            var phone = newPhone.Trim();
            var place = await Db.Places.FindAsync(selectedPlace.Id);
            if (place is null) return;

            place.Phone = phone;
            await Db.SaveChangesAsync();
            selectedPlace.Phone = phone;
            newPhone = phone;
            phoneContributionMessage = "전화번호를 저장했습니다.";
        }

        private async Task HandlePlaceImageSelection(InputFileChangeEventArgs args)
        {
            placeContributionMessage = "";
            if (args.FileCount is < 1 or > 5)
            {
                placeImageFiles.Clear();
                placeContributionMessage = "사진은 한 번에 1~5장까지 추가할 수 있어요.";
                return;
            }
            placeImageFiles = args.GetMultipleFiles(5).ToList();
            if (placeImageFiles.Any(file => file.Size <= 0 || file.Size > MaxCommentImageSize))
            {
                placeImageFiles.Clear();
                placeContributionMessage = "사진은 장당 5MB까지 추가할 수 있어요.";
            }
            else
            {
                placeContributionMessage = $"사진 {placeImageFiles.Count}장을 선택했습니다.";
                await UploadPlaceImagesAsync();
            }
        }

        private async Task UploadPlaceImagesAsync()
        {
            if (selectedPlace is null || placeImageFiles.Count == 0) return;
            var savedPaths = new List<string>();
            try
            {
                foreach (var file in placeImageFiles)
                {
                    var path = await SafeImageStorage.SaveAsync(file, Environment.WebRootPath, $"uploads/places/{selectedPlace.Id}", MaxCommentImageSize);
                    savedPaths.Add(path);
                    Db.PlaceImages.Add(new PlaceImage { PlaceId = selectedPlace.Id, FilePath = path });
                }
            }
            catch (Exception exception) when (exception is InvalidDataException or IOException)
            {
                foreach (var path in savedPaths) SafeImageStorage.Delete(Environment.WebRootPath, path);
                var savedImages = Db.PlaceImages.Local.Where(image => savedPaths.Contains(image.FilePath)).ToList();
                Db.PlaceImages.RemoveRange(savedImages);
                placeContributionMessage = "JPG, PNG, GIF, WEBP 사진만 가능하며 장당 5MB까지 추가할 수 있어요.";
                return;
            }
            await Db.SaveChangesAsync();
            selectedPlaceImages = await Db.PlaceImages.Where(image => image.PlaceId == selectedPlace.Id).Select(image => image.FilePath).ToListAsync();
            placeImageFiles.Clear();
            placeContributionMessage = "사진을 추가했습니다.";
        }

}
