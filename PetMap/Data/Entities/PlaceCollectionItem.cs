namespace PetMap.Data.Entities;

/// <summary>폴더에 저장한 장소와 개인 메모.</summary>
public class PlaceCollectionItem
{
    public int CollectionId { get; set; }
    public int PlaceId { get; set; }
    public string Memo { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
