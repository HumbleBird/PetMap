namespace PetMap.Data.Entities;

/// <summary>장소 상세 패널을 연 기록.</summary>
public class PlaceView
{
    public long Id { get; set; }
    public int PlaceId { get; set; }
    public string? UserId { get; set; }
    public DateTime ViewedAtUtc { get; set; } = DateTime.UtcNow;
}
