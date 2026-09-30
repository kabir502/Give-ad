using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NGO_website.Models.Entities;

namespace NGO_website.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NGO> NGOs => Set<NGO>();
        public DbSet<Cause> Causes => Set<Cause>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Programme> Programmes => Set<Programme>();
        public DbSet<ProgrammeInterest> ProgrammeInterests => Set<ProgrammeInterest>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();
        public DbSet<Partner> Partners => Set<Partner>();
        public DbSet<Query> Queries => Set<Query>();
        public DbSet<QueryReply> QueryReplies => Set<QueryReply>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<FAQ> FAQs => Set<FAQ>();
        public DbSet<AboutUsContent> AboutUsContents => Set<AboutUsContent>();
        public DbSet<FriendInvitation> FriendInvitations => Set<FriendInvitation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Indexes for fast lookup and integrity
            builder.Entity<Donation>()
                .HasIndex(d => d.DonationNumber)
                .IsUnique();

            builder.Entity<Payment>()
                .HasIndex(p => p.PaymentReference)
                .IsUnique();

            // 1-to-1 relationship between Donation and Payment
            builder.Entity<Donation>()
                .HasOne(d => d.Payment)
                .WithOne(p => p.Donation)
                .HasForeignKey<Payment>(p => p.DonationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Financial & User Relationships: Avoid cascading deletes on financial records
            builder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.ApprovedBy)
                .WithMany()
                .HasForeignKey(d => d.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.RejectedBy)
                .WithMany()
                .HasForeignKey(d => d.RejectedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.VerifiedBy)
                .WithMany()
                .HasForeignKey(p => p.VerifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.NGO)
                .WithMany(n => n.Donations)
                .HasForeignKey(d => d.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.Cause)
                .WithMany(c => c.Donations)
                .HasForeignKey(d => d.CauseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.Campaign)
                .WithMany(c => c.Donations)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cause -> NGO
            builder.Entity<Cause>()
                .HasOne(c => c.NGO)
                .WithMany(n => n.Causes)
                .HasForeignKey(c => c.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            // Campaign -> NGO & Cause
            builder.Entity<Campaign>()
                .HasOne(c => c.NGO)
                .WithMany(n => n.Campaigns)
                .HasForeignKey(c => c.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Campaign>()
                .HasOne(c => c.Cause)
                .WithMany(cause => cause.Campaigns)
                .HasForeignKey(c => c.CauseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Programme -> NGO
            builder.Entity<Programme>()
                .HasOne(p => p.NGO)
                .WithMany(n => n.Programmes)
                .HasForeignKey(p => p.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProgrammeInterest -> Programme & User
            builder.Entity<ProgrammeInterest>()
                .HasOne(pi => pi.Programme)
                .WithMany(p => p.ProgrammeInterests)
                .HasForeignKey(pi => pi.ProgrammeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgrammeInterest>()
                .HasOne(pi => pi.User)
                .WithMany(u => u.ProgrammeInterests)
                .HasForeignKey(pi => pi.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event -> NGO
            builder.Entity<Event>()
                .HasOne(e => e.NGO)
                .WithMany(n => n.Events)
                .HasForeignKey(e => e.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            // GalleryItem -> NGO & Programme
            builder.Entity<GalleryItem>()
                .HasOne(g => g.NGO)
                .WithMany(n => n.GalleryItems)
                .HasForeignKey(g => g.NGOId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GalleryItem>()
                .HasOne(g => g.Programme)
                .WithMany(p => p.GalleryItems)
                .HasForeignKey(g => g.ProgrammeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query -> ApplicationUser & QueryReply
            builder.Entity<Query>()
                .HasOne(q => q.User)
                .WithMany(u => u.Queries)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QueryReply>()
                .HasOne(qr => qr.Query)
                .WithMany(q => q.Replies)
                .HasForeignKey(qr => qr.QueryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QueryReply>()
                .HasOne(qr => qr.AdminUser)
                .WithMany()
                .HasForeignKey(qr => qr.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FriendInvitation -> SenderUser
            builder.Entity<FriendInvitation>()
                .HasOne(f => f.SenderUser)
                .WithMany(u => u.FriendInvitations)
                .HasForeignKey(f => f.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
