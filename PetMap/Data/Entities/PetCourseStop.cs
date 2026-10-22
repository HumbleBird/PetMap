namespace PetMap.Data.Entities;

/// <summary>코스 안의 장소와 방문 순서.</summary>
public class PetCourseStop
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int PlaceId { get; set; }
    public int SortOrder { get; set; }
    public string Memo { get; set; } = string.Empty;
}
