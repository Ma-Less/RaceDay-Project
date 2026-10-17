using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CaptureResultDto
    {
        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishPosition { get; set; }
    }
}