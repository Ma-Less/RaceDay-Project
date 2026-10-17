using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateEnrolmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, Cancelled
    }
}