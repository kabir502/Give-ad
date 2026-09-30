using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// User participation or registration interest for a programme
    /// </summary>
    public class ProgrammeInterest
    {
        [Key]
        public int ProgrammeInterestId { get; set; }

        [Required]
        public int ProgrammeId { get; set; }

        [ForeignKey("ProgrammeId")]
        public virtual Programme? Programme { get; set; }

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

        [Required]
        [Phone]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Notes / Message")]
        public string? Notes { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Confirmed"; // Confirmed, Attended, Cancelled
    }
}
