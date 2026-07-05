using System.ComponentModel.DataAnnotations;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel {
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    public RegisterModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager) {
        _context = context;
        _userManager = userManager;
        _userStore = userStore;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid) {
            return Page();
        }

        if (!AppRoles.IsSelectable(Input.AccountType)) {
            ModelState.AddModelError("Input.AccountType", "Choose a valid account type.");
            return Page();
        }

        var user = new ApplicationUser {
            FullName = $"{Input.FirstName.Trim()} {Input.LastName.Trim()}",
            CreatedAt = DateTime.UtcNow
        };

        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        _context.RoommateProfiles.Add(new RoommateProfile {
            UserId = user.Id,
            DateOfBirth = Input.DateOfBirth,
            City = Input.City.Trim()
        });
        await _context.SaveChangesAsync();

        var roleResult = await _userManager.AddToRoleAsync(user, Input.AccountType);
        if (!roleResult.Succeeded) {
            await transaction.RollbackAsync();
            foreach (var error in roleResult.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await transaction.CommitAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore() {
        if (!_userManager.SupportsUserEmail) {
            throw new NotSupportedException("The registration flow requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }

    public sealed class InputModel {
        [Required]
        [StringLength(49, MinimumLength = 2)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(49, MinimumLength = 2)]
        [Display(Name = "Surname")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        [ValidDateOfBirth]
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "I want to")]
        public string AccountType { get; set; } = AppRoles.Student;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
