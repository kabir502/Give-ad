using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Editable About Us CMS section content
    /// </summary>
    public class AboutUsContent
    {
        [Key]
        public int AboutUsContentId { get; set; }

        [Required]
        [StringLength(50)]
        public string SectionKey { get; set; } = string.Empty; // WhatWeDo, OurMission, OurTeam, CareerWithUs, OurAchievements, OurSupporters, ReadAboutUs

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Subtitle { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? IconClass { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
