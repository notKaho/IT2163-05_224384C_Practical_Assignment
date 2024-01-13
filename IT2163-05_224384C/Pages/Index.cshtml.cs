using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims;

namespace IT2163_05_224384C.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<Member> _userManager;
        private readonly SignInManager<Member> _signInManager; // Added SignInManager

        public IndexModel(ILogger<IndexModel> logger, UserManager<Member> userManager, SignInManager<Member> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager; // Assigning SignInManager
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Continue with your normal processing
            var user = await _userManager.GetUserAsync(User);
            //await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (user == null)
            {
                return NotFound(); // Handle the case where the user is not found
            }

            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");


            var email = HttpContext.Session.GetString("Email");

            // Pass user data to the view
            ViewData["Email"] = email;
            ViewData["FullName"] = user.FullName;
            ViewData["CreditCard"] = protector.Unprotect(user.CreditCard);
            ViewData["Gender"] = user.Gender;
            ViewData["DeliveryAddress"] = user.DeliveryAddress;
            ViewData["AboutMe"] = user.AboutMe;
            ViewData["PhoneNumber"] = user.PhoneNumber;
            ViewData["Photo"] = user.Photo;

            

            return Page();
        }


        
    }
}
