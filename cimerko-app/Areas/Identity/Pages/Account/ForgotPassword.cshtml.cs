using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using cimerko_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace cimerko_app.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender) {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool EmailSent { get; private set; }

    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) {
            return Page();
        }

        // Accounts do not need a confirmed email to sign in, so resets are not limited to confirmed ones either.
        var user = await _userManager.FindByEmailAsync(Email);
        if (user != null) {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme)!;

            await _emailSender.SendEmailAsync(
                Email,
                "Reset your Cimerko password",
                $"Someone asked to reset the password for your Cimerko account. " +
                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Choose a new password</a>. " +
                "If this was not you, you can ignore this email.");
        }

        // Same answer whether or not the account exists, so the form can't be used to look up emails.
        EmailSent = true;
        return Page();
    }
}
