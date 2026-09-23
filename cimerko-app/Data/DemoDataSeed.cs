using cimerko_app.Models;
using cimerko_app.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Data;

public static class DemoDataSeed {
    public const string DemoPassword = "Demo123!";

    private const string ListingImagePrefix = "/images/demo/listings/";
    private const string AvatarPrefix = "/images/demo/avatars/";

    private sealed record DemoUser(
        string Key,
        string FullName,
        string Email,
        string Role,
        string City,
        string Gender,
        DateOnly DateOfBirth,
        string? University,
        string? StudyProgram,
        string Bio,
        string Smoking,
        string Pets,
        string Cleanliness,
        string Sleep,
        string Guests,
        int JoinedDaysAgo);

    private sealed record DemoListing(
        string OwnerKey,
        string Title,
        ListingType Type,
        string City,
        string? Address,
        decimal Rent,
        int Rooms,
        int? Bedrooms,
        string Description,
        string[] Images,
        int CreatedDaysAgo,
        int? AvailableInDays,
        int? RoommatesNeeded = null,
        RoommateHousingPlan? HousingPlan = null,
        RoommateGenderPreference GenderPreference = RoommateGenderPreference.NoPreference,
        bool PetFriendly = false,
        bool SmokeFree = false,
        bool EarlyBird = false,
        bool NightOwl = false,
        bool Tidy = false,
        bool GuestsWelcome = false,
        TenantTypePreference Tenant = TenantTypePreference.NoPreference,
        RentalSmokingPolicy SmokingPolicy = RentalSmokingPolicy.NotSpecified,
        RentalPetPolicy PetPolicy = RentalPetPolicy.NotSpecified);

    private static readonly DemoUser[] Users = [
        new("mila", "Mia Thompson", "mia.thompson@cimerko.local", AppRoles.Student, "Skopje", "Female",
            new DateOnly(2004, 3, 14), "Ss. Cyril and Methodius University", "FINKI",
            "Second-year software engineering student. I spend most evenings at the library or cooking, and I like a calm flat where everyone respects quiet hours.",
            "No smoking", "Pets are welcome", "Very tidy", "Early bird", "Occasionally", 12),
        new("nikola", "Lucas Martin", "lucas.martin@cimerko.local", AppRoles.Student, "Skopje", "Male",
            new DateOnly(2002, 9, 2), "Ss. Cyril and Methodius University", "Faculty of Economics",
            "Economics student and part-time barista. Easy-going, sociable, and always up for a shared dinner. I keep common spaces clean and expect the same.",
            "Outside only", "Ask first", "Balanced", "Night owl", "Often", 40),
        new("ana", "Anna Schmidt", "anna.schmidt@cimerko.local", AppRoles.Student, "Štip", "Female",
            new DateOnly(2005, 1, 27), "Goce Delčev University", "Faculty of Medical Sciences",
            "Medical student with early lectures and a lot of studying. I am looking for a quiet, non-smoking flatmate who does not mind a very organised kitchen.",
            "No smoking", "No pets", "Very tidy", "Early bird", "Rarely", 6),
        new("stefan", "Daniel Weber", "daniel.weber@cimerko.local", AppRoles.Student, "Bitola", "Male",
            new DateOnly(2003, 6, 8), "St. Kliment Ohridski University", "Faculty of Technical Sciences",
            "Engineering student who plays guitar (with headphones, mostly). Looking for someone to search for a flat together near the centre of Bitola.",
            "No smoking", "Pets are welcome", "Balanced", "Flexible", "Occasionally", 25),
        new("elena", "Sofia Rossi", "sofia.rossi@cimerko.local", AppRoles.Student, "Skopje", "Female",
            new DateOnly(2001, 11, 19), "Ss. Cyril and Methodius University", "Faculty of Architecture",
            "Final-year architecture student. I work late on models and drawings, so I value a tidy, calm home and a flatmate who is fine with a night owl.",
            "No smoking", "Ask first", "Very tidy", "Night owl", "Rarely", 90),
        new("marko", "Mark Johnson", "mark.johnson@cimerko.local", AppRoles.Landlord, "Skopje", "Male",
            new DateOnly(1991, 4, 3), null, null,
            "I rent out a few apartments in central Skopje, mostly to students and young professionals. Quick to answer and happy to help with paperwork.",
            "No smoking", "Ask first", "Balanced", "Flexible", "Occasionally", 200),
        new("jana", "Laura Bennett", "laura.bennett@cimerko.local", AppRoles.Landlord, "Ohrid", "Female",
            new DateOnly(1984, 8, 22), null, null,
            "Family apartment in Ohrid that we rent to students during the academic year. Fully furnished, close to the lake and the old town.",
            "No smoking", "No pets", "Very tidy", "Early bird", "Rarely", 150),
        new("bojan", "David Miller", "david.miller@cimerko.local", AppRoles.Landlord, "Bitola", "Male",
            new DateOnly(1980, 2, 11), null, null,
            "Long-time Bitola resident renting a renovated studio a short walk from the university. Bills are included so there are no surprises.",
            "Outside only", "Pets are welcome", "Balanced", "Flexible", "Often", 120),
        new("teodora", "Emma Laurent", "emma.laurent@cimerko.local", AppRoles.Student, "Tetovo", "Female",
            new DateOnly(2004, 7, 30), "South East European University", "Faculty of Contemporary Sciences and Technologies",
            "Computer science student who loves plants and board games. Our shared house has one room free from October and we would love a friendly, tidy housemate.",
            "No smoking", "Pets are welcome", "Balanced", "Flexible", "Often", 18),
        new("dario", "Oliver Hughes", "oliver.hughes@cimerko.local", AppRoles.Landlord, "Skopje", "Male",
            new DateOnly(1996, 12, 5), null, null,
            "Designer by day, landlord of two small flats in Skopje. I like renting to people who treat the place like their own home.",
            "No smoking", "Ask first", "Very tidy", "Night owl", "Occasionally", 60)
    ];

    private static readonly DemoListing[] Listings = [
        new("mila", "Bright room in a 2-bedroom flat in Debar Maalo", ListingType.LookingForRoommate, "Skopje",
            "Debar Maalo", 180, 3, 2,
            "One room is free in the flat I share near Debar Maalo. The room has a double bed, a desk by the window and plenty of storage. The living room gets sun all afternoon and the kitchen is fully equipped. Five minutes to the FINKI bus and ten minutes on foot to the city centre. Bills are split evenly and usually come to around 40 € each.",
            ["debar-maalo-room-1.jpg", "debar-maalo-room-2.jpg", "debar-maalo-room-3.jpg"], 3, 14,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.HavePlace,
            GenderPreference: RoommateGenderPreference.Women, PetFriendly: true, SmokeFree: true, EarlyBird: true, Tidy: true),
        new("nikola", "Flatmate wanted for a 3-room apartment in Karpoš", ListingType.LookingForRoommate, "Skopje",
            "Karpoš 3", 160, 3, 2,
            "My flatmate is moving abroad so a furnished room is free from next month. The apartment is on the fourth floor with an elevator, a big balcony and fast internet. I work shifts at a café and study economics, so the place is quiet most mornings. Looking for someone social who is happy to share the occasional dinner.",
            ["karpos-flat-1.jpg", "karpos-flat-2.jpg", "karpos-flat-3.jpg"], 8, 30,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.HavePlace, GuestsWelcome: true, NightOwl: true),
        new("ana", "Room next to the campus in Štip, quiet flatmate wanted", ListingType.LookingForRoommate, "Štip",
            "Goce Delčev campus", 120, 2, 2,
            "Two-bedroom flat a three-minute walk from the university. The free room is furnished with a single bed, desk and wardrobe. I keep early hours because of lectures and clinical practice, so the ideal flatmate is tidy, non-smoking and does not host parties on weekdays. Heating and water are included in the rent.",
            ["stip-campus-1.jpg", "stip-campus-2.jpg", "stip-campus-3.jpg"], 1, 7,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.HavePlace, SmokeFree: true, EarlyBird: true, Tidy: true),
        new("stefan", "Let's find a flat together near Širok Sokak", ListingType.LookingForRoommate, "Bitola",
            null, 150, 2, 1,
            "I do not have a place yet, but I have shortlisted a few two-bedroom flats around Širok Sokak and the technical faculty. Budget is around 150 € each plus bills. I am relaxed, keep the kitchen clean and play guitar with headphones. If you want to split the search and the deposit, message me and we can go see the flats together.",
            ["bitola-search-1.jpg", "bitola-search-2.jpg"], 15, 45,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.SearchTogether, PetFriendly: true, SmokeFree: true),
        new("elena", "Quiet room in Aerodrom for a tidy flatmate", ListingType.LookingForRoommate, "Skopje",
            "Aerodrom, Jane Sandanski", 170, 3, 2,
            "Spacious room in a calm apartment in Aerodrom, close to the Jane Sandanski arena and the bus lines to the centre. I am an architecture student and often work late on projects, so I keep the place clean and quiet. The room has a large window, a double bed and a big desk. No smoking indoors.",
            ["aerodrom-room-1.jpg", "aerodrom-room-2.jpg", "aerodrom-room-3.jpg"], 21, null,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.HavePlace, SmokeFree: true, NightOwl: true, Tidy: true),
        new("teodora", "Room free in a shared house near SEEU campus", ListingType.LookingForRoommate, "Tetovo",
            "Ilindenska", 110, 4, 3,
            "We are three students sharing a house ten minutes from the SEEU campus, and one room opens up in October. The house has a garden, a big shared kitchen and a living room full of plants. We cook together a few times a week and host a board game night on Fridays. Pets are welcome if they get along with our cat.",
            ["tetovo-house-1.jpg", "tetovo-house-2.jpg", "tetovo-house-3.jpg"], 5, 20,
            RoommatesNeeded: 1, HousingPlan: RoommateHousingPlan.HavePlace, PetFriendly: true, SmokeFree: true, GuestsWelcome: true),
        new("marko", "Sunny studio in Centar, walking distance to everything", ListingType.PlaceForRent, "Skopje",
            "Centar, Dame Gruev", 320, 1, 0,
            "Compact, renovated studio on the third floor of a quiet building in the centre. It comes with a comfortable sofa bed, a small kitchen with induction hob, a washing machine and fast fibre internet. The tram, the university and the city park are all within a ten-minute walk. Ideal for a single student or young professional.",
            ["centar-studio-1.jpg", "centar-studio-2.jpg", "centar-studio-3.jpg"], 2, null,
            Tenant: TenantTypePreference.Student, SmokingPolicy: RentalSmokingPolicy.SmokeFree, PetPolicy: RentalPetPolicy.NoPets),
        new("marko", "Renovated 2-bedroom apartment in Kapištec", ListingType.PlaceForRent, "Skopje",
            "Kapištec, Nikola Parapunov", 520, 4, 2,
            "Fully renovated two-bedroom apartment perfect for two students sharing. Open-plan kitchen and living room, two equal-sized bedrooms with double beds and desks, and a modern bathroom with a walk-in shower. Central heating, air conditioning and a parking spot in the yard. Close to the medical faculty and Vodno.",
            ["kapistec-2br-3.jpg", "kapistec-2br-2.jpg", "kapistec-2br-1.jpg", "kapistec-2br-4.jpg"], 9, 10,
            Tenant: TenantTypePreference.Student, SmokingPolicy: RentalSmokingPolicy.OutsideOnly, PetPolicy: RentalPetPolicy.AskLandlord),
        new("marko", "Family apartment with a big balcony in Karpoš 4", ListingType.PlaceForRent, "Skopje",
            "Karpoš 4", 650, 5, 3,
            "Three-bedroom apartment with a wide balcony overlooking the park in Karpoš 4. Bright living room, separate kitchen with all appliances, two bathrooms and plenty of storage. Works well for three students sharing or a small family. The City Mall, the bus terminal and several faculties are a short ride away.",
            ["karpos4-family-1.jpg", "karpos4-family-2.jpg", "karpos4-family-3.jpg"], 30, 25,
            SmokingPolicy: RentalSmokingPolicy.SmokeFree, PetPolicy: RentalPetPolicy.Allowed),
        new("jana", "Lake-view apartment ten minutes from Ohrid old town", ListingType.PlaceForRent, "Ohrid",
            "Kej Makedonija", 380, 3, 2,
            "Furnished two-bedroom apartment on the lake promenade, rented to students for the academic year. Every room has a view of the lake, the kitchen is fully equipped and there is a washing machine and a dishwasher. The old town, the university buildings and the bus station are all within a ten-minute walk.",
            ["ohrid-lake-1.jpg", "ohrid-lake-2.jpg", "ohrid-lake-3.jpg"], 12, 12,
            Tenant: TenantTypePreference.Student, SmokingPolicy: RentalSmokingPolicy.SmokeFree, PetPolicy: RentalPetPolicy.NoPets),
        new("bojan", "Furnished studio near the university in Bitola, bills included", ListingType.PlaceForRent, "Bitola",
            "Bulevar 1 Maj", 220, 1, 0,
            "Renovated studio five minutes from the technical faculty and the main pedestrian street. The price includes heating, water, electricity and internet, so the monthly cost is fixed. Comes with a double bed, a wardrobe, a desk, a kitchenette and a new bathroom. A quiet street with a bakery and a supermarket around the corner.",
            ["bitola-studio-1.jpg", "bitola-studio-2.jpg", "bitola-studio-3.jpg"], 4, null,
            Tenant: TenantTypePreference.Student, SmokingPolicy: RentalSmokingPolicy.OutsideOnly, PetPolicy: RentalPetPolicy.Allowed),
        new("dario", "Modern loft in Debar Maalo with a home office corner", ListingType.PlaceForRent, "Skopje",
            "Debar Maalo, Orce Nikolov", 480, 2, 1,
            "A bright, open loft in a renovated building in the heart of Debar Maalo. High ceilings, a mezzanine bedroom, a large desk area with a second monitor, and a compact kitchen. Cafés, bakeries and the city park are literally downstairs. Suits a single tenant or a couple who work or study from home.",
            ["debar-loft-1.jpg", "debar-loft-2.jpg", "debar-loft-3.jpg", "debar-loft-4.jpg"], 6, 18,
            Tenant: TenantTypePreference.WorkingProfessional, SmokingPolicy: RentalSmokingPolicy.SmokeFree, PetPolicy: RentalPetPolicy.AskLandlord),
        new("dario", "Cosy 1-bedroom in Taftalidže near the shopping centre", ListingType.PlaceForRent, "Skopje",
            "Taftalidže 2", 400, 2, 1,
            "Warm one-bedroom apartment on a green, quiet street in Taftalidže. Separate bedroom with a double bed, a living room with a dining table and sofa, and a kitchen with a dishwasher. Ten minutes by bus to the university campus and a short walk to Skopje City Mall. Parking available on the street.",
            ["taftalidze-1br-1.jpg", "taftalidze-1br-2.jpg", "taftalidze-1br-3.jpg"], 18, null,
            SmokingPolicy: RentalSmokingPolicy.SmokeFree, PetPolicy: RentalPetPolicy.NoPets)
    ];

    public static async Task SeedAsync(IServiceProvider services) {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await context.Users.AnyAsync(user => user.IsDemoUser)) {
            return;
        }

        var now = DateTime.UtcNow;
        var createdUsers = new Dictionary<string, ApplicationUser>();

        foreach (var demoUser in Users) {
            var user = new ApplicationUser {
                UserName = demoUser.Email,
                Email = demoUser.Email,
                EmailConfirmed = true,
                FullName = demoUser.FullName,
                ProfileImageUrl = $"{AvatarPrefix}{demoUser.Key}.jpg",
                IsDemoUser = true,
                CreatedAt = now.AddDays(-demoUser.JoinedDaysAgo),
                RoommateProfile = new RoommateProfile {
                    Bio = demoUser.Bio,
                    ContactEmail = demoUser.Email,
                    DateOfBirth = demoUser.DateOfBirth,
                    City = demoUser.City,
                    Gender = demoUser.Gender,
                    University = demoUser.University,
                    StudyProgram = demoUser.StudyProgram,
                    SmokingPreference = demoUser.Smoking,
                    PetsPreference = demoUser.Pets,
                    CleanlinessLevel = demoUser.Cleanliness,
                    SleepSchedule = demoUser.Sleep,
                    GuestPreference = demoUser.Guests
                }
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded) {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create demo user {demoUser.Email}. {errors}");
            }

            await userManager.AddToRoleAsync(user, demoUser.Role);
            createdUsers[demoUser.Key] = user;
        }

        var createdListings = new List<Listing>();
        foreach (var demoListing in Listings) {
            var createdAt = now.AddDays(-demoListing.CreatedDaysAgo);
            var listing = new Listing {
                Title = demoListing.Title,
                Description = demoListing.Description,
                Type = demoListing.Type,
                City = demoListing.City,
                Address = demoListing.Address,
                ContactPhone = "+389 70 000 000",
                MonthlyRent = demoListing.Rent,
                RoomCount = demoListing.Rooms,
                BedroomCount = demoListing.Bedrooms,
                RoommatesNeeded = demoListing.RoommatesNeeded,
                TenantTypePreference = demoListing.Tenant,
                RentalSmokingPolicy = demoListing.SmokingPolicy,
                RentalPetPolicy = demoListing.PetPolicy,
                RoommateGenderPreference = demoListing.GenderPreference,
                RoommateHousingPlan = demoListing.HousingPlan,
                RoommatePetFriendly = demoListing.PetFriendly,
                RoommateSmokeFree = demoListing.SmokeFree,
                RoommateEarlyBird = demoListing.EarlyBird,
                RoommateNightOwl = demoListing.NightOwl,
                RoommateTidy = demoListing.Tidy,
                RoommateGuestsWelcome = demoListing.GuestsWelcome,
                AvailableFrom = demoListing.AvailableInDays.HasValue
                    ? now.Date.AddDays(demoListing.AvailableInDays.Value)
                    : null,
                IsActive = true,
                ModerationStatus = ListingModerationStatus.Approved,
                CreatedAt = createdAt,
                OwnerId = createdUsers[demoListing.OwnerKey].Id
            };

            for (var index = 0; index < demoListing.Images.Length; index++) {
                listing.Images.Add(new ListingImage {
                    ImageUrl = $"{ListingImagePrefix}{demoListing.Images[index]}",
                    IsPrimary = index == 0,
                    CreatedAt = createdAt.AddSeconds(index)
                });
            }

            createdListings.Add(listing);
        }

        context.Listings.AddRange(createdListings);
        await context.SaveChangesAsync();

        // Accepted requests unlock reviews, so a few demo members have already lived together.
        var reviewPairs = new (string Reviewer, string Reviewed, string ListingTitle, int Rating, string Comment)[] {
            ("mila", "marko", "Sunny studio in Centar, walking distance to everything", 5,
                "Mark fixed the boiler the same day I reported it and always answered within an hour. The studio was exactly as described."),
            ("nikola", "marko", "Renovated 2-bedroom apartment in Kapištec", 4,
                "Clean apartment and a fair deposit process. The internet was slow the first week but Mark sorted it quickly."),
            ("elena", "marko", "Family apartment with a big balcony in Karpoš 4", 5,
                "Rented for a full academic year with two friends. No surprises with bills and the balcony was our favourite study spot."),
            ("stefan", "bojan", "Furnished studio near the university in Bitola, bills included", 5,
                "Bills really are included, which made budgeting easy. David is friendly and respects your privacy."),
            ("ana", "jana", "Lake-view apartment ten minutes from Ohrid old town", 4,
                "Beautiful view and a very well equipped kitchen. A bit far from campus on rainy days, but the bus is frequent."),
            ("nikola", "mila", "Bright room in a 2-bedroom flat in Debar Maalo", 5,
                "Shared a flat with Mia for a semester. Tidy, considerate and great at splitting chores fairly."),
            ("teodora", "elena", "Quiet room in Aerodrom for a tidy flatmate", 4,
                "Sofia keeps a very calm home. She works late but is quiet about it, and the kitchen was always spotless.")
        };

        foreach (var pair in reviewPairs) {
            var listing = createdListings.First(item => item.Title == pair.ListingTitle);
            var reviewer = createdUsers[pair.Reviewer];
            var reviewed = createdUsers[pair.Reviewed];

            context.ListingRequests.Add(new ListingRequest {
                ListingId = listing.Id,
                SenderId = reviewer.Id,
                Message = "Hi! I saw your listing on Cimerko and would love to come and see the place this week.",
                Status = RequestStatus.Accepted,
                CreatedAt = listing.CreatedAt.AddHours(6)
            });

            var reviewedAt = listing.CreatedAt.AddDays(20);
            if (reviewedAt > now) {
                reviewedAt = now.AddDays(-1);
            }


            context.Reviews.Add(new Review {
                ReviewerId = reviewer.Id,
                ReviewedUserId = reviewed.Id,
                Rating = pair.Rating,
                SmokingRating = 5,
                PetsRating = pair.Rating,
                CleanlinessRating = pair.Rating,
                SleepScheduleRating = Math.Max(3, pair.Rating - 1),
                GuestPreferenceRating = pair.Rating,
                Comment = pair.Comment,
                CreatedAt = reviewedAt
            });
        }

        await context.SaveChangesAsync();
    }
}
