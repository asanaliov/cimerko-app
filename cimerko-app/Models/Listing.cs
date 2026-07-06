using System.ComponentModel.DataAnnotations;
using cimerko_app.Models.Enums;

namespace cimerko_app.Models;

public class Listing {
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Listing type")]
    [EnumDataType(typeof(ListingType), ErrorMessage = "Choose a valid listing type.")]
    public ListingType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [Required]
    [Phone]
    [MaxLength(30)]
    [Display(Name = "Contact phone number")]
    public string ContactPhone { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal MonthlyRent { get; set; }

    [Range(1, 20)]
    public int RoomCount { get; set; }

    [Range(0, 20)]
    [Display(Name = "Bedrooms")]
    public int? BedroomCount { get; set; }

    [Range(1, 10)]
    [Display(Name = "Roommates needed")]
    public int? RoommatesNeeded { get; set; }

    [Display(Name = "Preferred tenant")]
    [EnumDataType(typeof(TenantTypePreference))]
    public TenantTypePreference TenantTypePreference { get; set; }

    [Display(Name = "Smoking policy")]
    [EnumDataType(typeof(RentalSmokingPolicy))]
    public RentalSmokingPolicy RentalSmokingPolicy { get; set; }

    [Display(Name = "Pet policy")]
    [EnumDataType(typeof(RentalPetPolicy))]
    public RentalPetPolicy RentalPetPolicy { get; set; }

    [Display(Name = "Preferred roommate gender")]
    [EnumDataType(typeof(RoommateGenderPreference))]
    public RoommateGenderPreference RoommateGenderPreference { get; set; }

    [Display(Name = "Housing plan")]
    [EnumDataType(typeof(RoommateHousingPlan))]
    public RoommateHousingPlan? RoommateHousingPlan { get; set; }

    [Display(Name = "Pet-friendly home")]
    public bool RoommatePetFriendly { get; set; }

    [Display(Name = "Smoke-free home")]
    public bool RoommateSmokeFree { get; set; }

    [Display(Name = "Early-bird routine")]
    public bool RoommateEarlyBird { get; set; }

    [Display(Name = "Night-owl routine")]
    public bool RoommateNightOwl { get; set; }

    [Display(Name = "Tidy shared spaces")]
    public bool RoommateTidy { get; set; }

    [Display(Name = "Guests are welcome")]
    public bool RoommateGuestsWelcome { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public bool IsActive { get; set; } = true;

    [Display(Name = "Moderation status")]
    public ListingModerationStatus ModerationStatus { get; set; } = ListingModerationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser? Owner { get; set; }

    public ICollection<SavedListing> SavedByUsers { get; set; } = new List<SavedListing>();

    public ICollection<ListingRequest> Requests { get; set; } = new List<ListingRequest>();

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();

    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
