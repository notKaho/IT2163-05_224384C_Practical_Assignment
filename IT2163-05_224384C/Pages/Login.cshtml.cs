using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IT2163_05_224384C.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;


namespace IT2163_05_224384C.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<Member> signInManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(SignInManager<Member> signInManager, IHttpContextAccessor httpContextAccessor, ILogger<LoginModel> logger)
        {
            this.signInManager = signInManager;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);

                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        logger.LogWarning($"User {user.Email} attempted to log in without confirming their email.");
                        ModelState.AddModelError("login", "Your email address is not confirmed.");
                        return Page();
                    }

                    var result = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        // Audit Log
                        user.LastLoginDate = DateTime.UtcNow;
                        await signInManager.UserManager.UpdateAsync(user);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, LModel.Email),
                            new Claim("Membership", "Member")
                        };

                        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync("MyCookieAuth", principal);

                        httpContextAccessor.HttpContext.Session.SetString("Email", LModel.Email);

                        return RedirectToPage("Index");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("login", "Account locked out due to too many failed attempts. Please try again later.");
                        return Page();
                    }
                }

                ModelState.AddModelError("login", "Username or Password incorrect");
            }

            return Page();
        }

    }
}