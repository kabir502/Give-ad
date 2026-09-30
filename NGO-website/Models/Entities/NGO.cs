using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Registered NGO organization entity
    /// </summary>
    public class NGO
    {
        [Key]
        public int NGOId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "NGO Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Logo URL / Path")]
        public string? LogoUrl { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Website { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Cause> Causes { get; set; } = new List<Cause>();
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
        public virtual ICollection<Programme> Programmes { get; set; } = new List<Programme>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
