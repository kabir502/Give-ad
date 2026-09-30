using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// User raised query / support ticket
    /// </summary>
    public class Query
    {
        [Key]
        public int QueryId { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Donation Support"; // Donation Support, Payment Issue, Programme Inquiry, General, Feedback

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        public QueryStatus Status { get; set; } = QueryStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<QueryReply> Replies { get; set; } = new List<QueryReply>();
    }
}
