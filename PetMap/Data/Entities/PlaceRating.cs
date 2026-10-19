namespace PetMap.Data.Entities;

/// <summary>사용자 한 명이 장소 하나에 남긴 별점.</summary>
public class PlaceRating
{
    public int Id { get; set; }
    public int PlaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Place Place { get; set; } = null!;
}
