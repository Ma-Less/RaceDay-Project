using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishPosition { get; set; }

        public DateTime DateCaptured { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }
    }
}