using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}