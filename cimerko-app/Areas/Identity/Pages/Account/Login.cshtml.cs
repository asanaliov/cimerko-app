using System.ComponentModel.DataAnnotations;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cimerko_app.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel {
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager) {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? returnUrl = null) {
        if (!string.IsNullOrEmpty(ErrorMessage)) {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid) {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded) {
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor) {
            return RedirectToPage("./LoginWith2fa", new {
                ReturnUrl = returnUrl,
                Input.RememberMe
            });
        }

        if (result.IsLockedOut) {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "The email or password is incorrect.");
        return Page();
    }

    public sealed class InputModel {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Keep me logged in")]
        public bool RememberMe { get; set; }
    }
}
