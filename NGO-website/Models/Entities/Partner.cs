using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Corporate, NGO, and institutional partners
    /// </summary>
    public class Partner
    {
        [Key]
        public int PartnerId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Partner Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Logo URL / Path")]
        public string? LogoUrl { get; set; }

        [StringLength(250)]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }

        [StringLength(100)]
        public string Category { get; set; } = "Strategic Partner"; // Strategic, Healthcare, Education, Tech, Corporate

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
