namespace PetMap.Data.Entities;

/// <summary>회원이 저장한 코스.</summary>
public class PetCourseFavorite
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
