using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Administrator response to a user query
    /// </summary>
    public class QueryReply
    {
        [Key]
        public int QueryReplyId { get; set; }

        [Required]
        public int QueryId { get; set; }

        [ForeignKey("QueryId")]
        public virtual Query? Query { get; set; }

        public string? AdminUserId { get; set; }

        [ForeignKey("AdminUserId")]
        public virtual ApplicationUser? AdminUser { get; set; }

        [Required]
        [StringLength(100)]
        public string AdminName { get; set; } = "GIVE-AID Support";

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        public DateTime RepliedAt { get; set; } = DateTime.UtcNow;
    }
}
