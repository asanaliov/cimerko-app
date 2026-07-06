using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using cimerko_app.Models;

namespace cimerko_app.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options) {

    public DbSet<RoommateProfile> RoommateProfiles { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<SavedListing> SavedListings { get; set; }
    public DbSet<ListingRequest> ListingRequests { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<RoommateProfile>()
            .HasOne(profile => profile.User)
            .WithOne(user => user.RoommateProfile)
            .HasForeignKey<RoommateProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Listing>()
            .HasOne(listing => listing.Owner)
            .WithMany(user => user.Listings)
            .HasForeignKey(listing => listing.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ListingImage>()
            .HasOne(image => image.Listing)
            .WithMany(listing => listing.Images)
            .HasForeignKey(image => image.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SavedListing>()
            .HasOne(savedListing => savedListing.User)
            .WithMany(user => user.SavedListings)
            .HasForeignKey(savedListing => savedListing.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SavedListing>()
            .HasOne(savedListing => savedListing.Listing)
            .WithMany(listing => listing.SavedByUsers)
            .HasForeignKey(savedListing => savedListing.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SavedListing>()
            .HasIndex(savedListing => new { savedListing.UserId, savedListing.ListingId })
            .IsUnique();

        builder.Entity<ListingRequest>()
            .HasOne(request => request.Listing)
            .WithMany(listing => listing.Requests)
            .HasForeignKey(request => request.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ListingRequest>()
            .HasOne(request => request.Sender)
            .WithMany(user => user.SentRequests)
            .HasForeignKey(request => request.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ListingRequest>()
            .HasIndex(request => new { request.SenderId, request.ListingId })
            .IsUnique();

        builder.Entity<Review>()
            .HasOne(review => review.Reviewer)
            .WithMany(user => user.ReviewsWritten)
            .HasForeignKey(review => review.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(review => review.ReviewedUser)
            .WithMany(user => user.ReviewsReceived)
            .HasForeignKey(review => review.ReviewedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasIndex(review => new { review.ReviewerId, review.ReviewedUserId })
            .IsUnique();

        builder.Entity<Report>()
            .HasOne(report => report.Reporter)
            .WithMany()
            .HasForeignKey(report => report.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Report>()
            .HasOne(report => report.ReportedUser)
            .WithMany()
            .HasForeignKey(report => report.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Report>()
            .HasOne(report => report.Listing)
            .WithMany(listing => listing.Reports)
            .HasForeignKey(report => report.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Report>()
            .HasOne(report => report.Review)
            .WithMany(review => review.Reports)
            .HasForeignKey(report => report.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Report>()
            .HasIndex(report => new { report.Status, report.CreatedAt });

        builder.Entity<Notification>()
            .HasOne(notification => notification.Recipient)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(notification => notification.Actor)
            .WithMany()
            .HasForeignKey(notification => notification.ActorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(notification => notification.Listing)
            .WithMany()
            .HasForeignKey(notification => notification.ListingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(notification => notification.ListingRequest)
            .WithMany()
            .HasForeignKey(notification => notification.ListingRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasIndex(notification => new {
                notification.RecipientId,
                notification.IsRead,
                notification.CreatedAt
            });
    }
}
