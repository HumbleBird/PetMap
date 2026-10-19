namespace PetMap.Data.Entities;

/// <summary>회원 또는 비회원이 장소에 남긴 댓글과 이미지 경로.</summary>
public class PlaceComment
{
    public int Id { get; set; }
    public int PlaceId { get; set; }
    public string? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? GuestPasswordHash { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImagePathsJson { get; set; }
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
