using System.ComponentModel.DataAnnotations;
using cimerko_app.Data;
using cimerko_app.Models;
using cimerko_app.Models.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace cimerko_app.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel {
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    public LoginModel(
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore) {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
    }

    public LoginInputModel Login { get; private set; } = new();

    public RegistrationInputModel Registration { get; private set; } = new();

    public string? ReturnUrl { get; set; }

    public string ActiveTab { get; private set; } = "login";

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? returnUrl = null, string? mode = null) {
        if (!string.IsNullOrEmpty(ErrorMessage)) {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ReturnUrl = returnUrl;
        ActiveTab = string.Equals(mode, "register", StringComparison.OrdinalIgnoreCase)
            ? "register"
            : "login";
    }

    public async Task<IActionResult> OnPostLoginAsync(LoginInputModel login, string? returnUrl = null) {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        Login = login;
        ActiveTab = "login";

        if (!ModelState.IsValid) {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            login.Email,
            login.Password,
            login.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded) {
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor) {
            return RedirectToPage("./LoginWith2fa", new {
                ReturnUrl = returnUrl,
                login.RememberMe
            });
        }

        if (result.IsLockedOut) {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "The email or password is incorrect.");
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync(
        RegistrationInputModel registration,
        string? returnUrl = null) {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        Registration = registration;
        ActiveTab = "register";

        if (!ModelState.IsValid) {
            return Page();
        }

        if (!AppRoles.IsSelectable(registration.AccountType)) {
            ModelState.AddModelError(
                "Registration.AccountType",
                "Choose a valid account type.");
            return Page();
        }

        var user = new ApplicationUser {
            FullName = $"{registration.FirstName.Trim()} {registration.LastName.Trim()}",
            CreatedAt = DateTime.UtcNow
        };

        await _userStore.SetUserNameAsync(user, registration.Email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, registration.Email, CancellationToken.None);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var result = await _userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        _context.RoommateProfiles.Add(new RoommateProfile {
            UserId = user.Id,
            DateOfBirth = registration.DateOfBirth,
            City = registration.City.Trim()
        });
        await _context.SaveChangesAsync();

        var roleResult = await _userManager.AddToRoleAsync(user, registration.AccountType);
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

    public sealed class LoginInputModel {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Keep me logged in")]
        public bool RememberMe { get; set; }
    }

    public sealed class RegistrationInputModel {
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
