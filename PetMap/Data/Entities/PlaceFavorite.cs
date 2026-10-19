namespace PetMap.Data.Entities;

/// <summary>회원이 찜한 장소.</summary>
public class PlaceFavorite
{
    public int PlaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
