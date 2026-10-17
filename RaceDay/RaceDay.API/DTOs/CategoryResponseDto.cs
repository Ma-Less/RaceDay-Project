namespace RaceDay.API.DTOs
{
    public class CategoryResponseDto
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}