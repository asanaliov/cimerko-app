using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cimerko_app.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel {
    public IActionResult OnGet(string? returnUrl = null) {
        return RedirectToPage("./Login", new {
            mode = "register",
            returnUrl
        });
    }
}
