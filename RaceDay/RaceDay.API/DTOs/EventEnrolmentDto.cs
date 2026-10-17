using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EnrolEventDto
    {
        [Required]
        public int CategoryID { get; set; }
    }
}