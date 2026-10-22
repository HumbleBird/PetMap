namespace PetMap.Data.Entities;

/// <summary>회원의 코스 추천 기록.</summary>
public class PetCourseLike
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
