using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// General public contact inquiry
    /// </summary>
    public class ContactMessage
    {
        [Key]
        public int ContactMessageId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [StringLength(4000)]
        public string? AdminReply { get; set; }

        public DateTime? RepliedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
