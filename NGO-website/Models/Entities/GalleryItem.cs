using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Photo gallery item representing activities and programmes of NGOs
    /// </summary>
    public class GalleryItem
    {
        [Key]
        public int GalleryItemId { get; set; }

        public int? NGOId { get; set; }

        [ForeignKey("NGOId")]
        public virtual NGO? NGO { get; set; }

        public int? ProgrammeId { get; set; }

        [ForeignKey("ProgrammeId")]
        public virtual Programme? Programme { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title / Caption")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Image URL / Path")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = "Activities"; // e.g. Activities, Relief Work, Education, Medical Camps

        public DateTime DateTaken { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
