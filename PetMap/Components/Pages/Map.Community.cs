using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PetMap.Components.Pages.Models;
using PetMap.Data.Entities;
using PetMap.Data.Utiles;
using System.Security.Claims;

namespace PetMap.Components.Pages;

public partial class Map
{
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
            await LoadSavedPlacesAsync(userId);
        }

        private async Task ToggleSavedPlacesAsync()
        {
            isSavedPlacesPanelOpen = !isSavedPlacesPanelOpen;
            if (!isSavedPlacesPanelOpen) return;
            savedPlacesMessage = "";

            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            isAuthenticated = !string.IsNullOrWhiteSpace(userId);
            if (isAuthenticated && userId is not null)
                await LoadSavedPlacesAsync(userId);
        }

        private async Task LoadSavedPlacesAsync(string userId)
        {
            savedPlaces = await (
                from favorite in Db.PlaceFavorites.AsNoTracking()
                join place in Db.Places.AsNoTracking() on favorite.PlaceId equals place.Id
                where favorite.UserId == userId
                orderby place.Address, place.Name
                select new SavedPlaceCard
                {
                    PlaceId = place.Id,
                    Name = place.Name,
                    Address = place.Address,
                    Category = place.Category,
                    ImageUrl = Db.PlaceImages.Where(image => image.PlaceId == place.Id)
                        .OrderBy(image => image.Id)
                        .Select(image => image.FilePath)
                        .FirstOrDefault(),
                    FolderName = favorite.FolderName,
                    Memo = favorite.Memo
                })
                .ToListAsync();
        }

        private async Task SaveFavoriteDetailsAsync(SavedPlaceCard savedPlace)
        {
            var userId = (await AuthStateProvider.GetAuthenticationStateAsync()).User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return;
            var favorite = await Db.PlaceFavorites.FindAsync(savedPlace.PlaceId, userId);
            if (favorite is null) return;
            favorite.FolderName = string.IsNullOrWhiteSpace(savedPlace.FolderName) ? null : savedPlace.FolderName.Trim();
            favorite.Memo = string.IsNullOrWhiteSpace(savedPlace.Memo) ? null : savedPlace.Memo.Trim();
            await Db.SaveChangesAsync();
            savedPlacesMessage = $"{savedPlace.Name} 저장 정보를 업데이트했습니다.";
        }

        private async Task RemoveFavoriteAsync(int placeId)
        {
            var userId = (await AuthStateProvider.GetAuthenticationStateAsync()).User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return;
            var favorite = await Db.PlaceFavorites.FindAsync(placeId, userId);
            if (favorite is not null)
            {
                Db.PlaceFavorites.Remove(favorite);
                await Db.SaveChangesAsync();
            }
            if (selectedPlaceId == placeId) isFavorite = false;
            await LoadSavedPlacesAsync(userId);
        }

        private async Task OpenSavedPlaceAsync(int placeId)
        {
            isSavedPlacesPanelOpen = false;
            if (_mapModule is not null)
                await _mapModule.InvokeVoidAsync("focusPlace", placeId);
            await OnMapPlaceSelected(placeId);
        }

        private static string GetSavedPlaceRegion(string address)
        {
            var parts = address.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 3 ? string.Join(' ', parts.Take(3)) : address;
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

}
