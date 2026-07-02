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
    }
}
