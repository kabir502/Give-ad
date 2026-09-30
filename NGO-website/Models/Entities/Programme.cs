using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// NGO welfare programme / outreach initiative
    /// </summary>
    public class Programme
    {
        [Key]
        public int ProgrammeId { get; set; }

        [Required]
        [Display(Name = "NGO")]
        public int NGOId { get; set; }

        [ForeignKey("NGOId")]
        public virtual NGO? NGO { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Programme Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.AddDays(7);

        [StringLength(50)]
        [Display(Name = "Start Time")]
        public string? StartTime { get; set; }

        [StringLength(50)]
        [Display(Name = "End Time")]
        public string? EndTime { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Banner / Image")]
        public string? ImageUrl { get; set; }

        [Range(1, 10000)]
        public int? Capacity { get; set; }

        public ProgrammeStatus Status { get; set; } = ProgrammeStatus.Upcoming;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<ProgrammeInterest> ProgrammeInterests { get; set; } = new List<ProgrammeInterest>();
        public virtual ICollection<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
    }
}
