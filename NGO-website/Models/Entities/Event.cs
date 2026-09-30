using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// NGO Public Event entity
    /// </summary>
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "NGO")]
        public int NGOId { get; set; }

        [ForeignKey("NGOId")]
        public virtual NGO? NGO { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date & Time")]
        public DateTime EventDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Image URL / Path")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
