using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Record of invitations sent by users to friends to invite them to join GIVE-AID causes
    /// </summary>
    public class FriendInvitation
    {
        [Key]
        public int FriendInvitationId { get; set; }

        public string? SenderUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual ApplicationUser? SenderUser { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Your Email")]
        public string SenderEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Friend's Name")]
        public string FriendName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Friend's Email")]
        public string FriendEmail { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Personal Message")]
        public string? PersonalMessage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
