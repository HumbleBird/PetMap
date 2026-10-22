namespace PetMap.Data.Entities;

/// <summary>사용자가 만든 반려동물 동반 코스.</summary>
public class PetCourse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<PetCourseStop> Stops { get; set; } = new List<PetCourseStop>();
}
