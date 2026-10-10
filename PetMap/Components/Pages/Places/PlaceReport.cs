namespace PetMap.Data.Entities;

public class PlaceReport
{
    public int Id { get; set; }

    // 제보자
    public string Nickname { get; set; } = "";

    // 장소 정보
    public string PlaceName { get; set; } = "";
    public string Category { get; set; } = "";

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? NaverPlaceUrl { get; set; }
    public string? BusinessHours { get; set; }

    // 제보 내용
    public string Review { get; set; } = "";
    public string? Memo { get; set; }

    // 검토 상태
    public Status Status { get; set; } = Status.Pending;

    // 관리자 검토 정보
    public string? AdminComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum Status
{
    Pending,
    Approved,
    Rejected
}