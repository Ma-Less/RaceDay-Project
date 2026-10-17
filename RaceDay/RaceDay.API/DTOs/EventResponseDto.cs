namespace RaceDay.API.DTOs
{
    public class EventResponseDto
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
        public bool IsActive { get; set; }

        public List<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();
    }
}