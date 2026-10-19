namespace PetMap.Data.Entities;

/// <summary>회원 댓글 좋아요 기록. 댓글/회원 조합은 한 번만 저장된다.</summary>
public class PlaceCommentLike
{
    public int CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
