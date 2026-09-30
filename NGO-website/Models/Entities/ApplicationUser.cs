using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Extended Identity User for GIVE-AID platform members and administrators
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Profession { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public virtual ICollection<Query> Queries { get; set; } = new List<Query>();
        public virtual ICollection<ProgrammeInterest> ProgrammeInterests { get; set; } = new List<ProgrammeInterest>();
        public virtual ICollection<FriendInvitation> FriendInvitations { get; set; } = new List<FriendInvitation>();
    }
}
