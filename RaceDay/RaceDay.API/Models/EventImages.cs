using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("EventImages")]
    public class EventImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageURL { get; set; } = string.Empty;

        public bool IsBanner { get; set; } = false;

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("EventID")]
        public Event? Event { get; set; }
    }
}