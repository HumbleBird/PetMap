namespace PetMap.Data.Entities
{
    public class PlaceImage
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public Place? Place { get; set; }
    }
}
