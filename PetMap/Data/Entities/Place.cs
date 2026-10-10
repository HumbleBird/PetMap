namespace PetMap.Data.Entities;

public class Place
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? RoadAddress { get; set; }

    public string? JibunAddress { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? NaverPlaceUrl { get; set; }

    public string? BusinessHours { get; set; }

    public string? Nickname { get; set; }

    public string? Review { get; set; }

    public string? Memo { get; set; }
}