using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FreshFarmMarket.ViewModels;
using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IT2163_05_224384C.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<Member> userManager;
        private readonly SignInManager<Member> signInManager;
        private readonly IConfiguration configuration;
        private readonly IHostEnvironment hostEnvironment;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<Member> userManager,
            SignInManager<Member> signInManager,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                // Handle invalid or missing parameters
                return Page();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Handle user not found
                return Page();
            }

            var result = await userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                // Email confirmed successfully
                TempData["EmailVerificationAlert"] = "Your email has been verified. You can now log in.";
                return RedirectToPage("/Login");
            }
            else
            {
                // Handle confirmation failure
                TempData["EmailVerificationAlert"] = "Email verification failed. Please try again.";
                return RedirectToPage("/Login");
            }
        }



        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid && RModel.Photo != null)
            {
                // Check if the email is already in use
                var existingUser = await userManager.FindByEmailAsync(RModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("RModel.Email", "Email is already in use.");
                    return Page();
                }

                var fileName = $"{Guid.NewGuid().ToString()}.jpg";
                var filePath = Path.Combine("wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await RModel.Photo.CopyToAsync(stream);
                }

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                var user = new Member()
                {
                    UserName = RModel.Email.Trim(),
                    Email = RModel.Email.Trim(),
                    FullName = RModel.FullName.Trim(),
                    CreditCard = protector.Protect(RModel.CreditCard).Trim(),
                    Gender = RModel.Gender.Trim(),
                    DeliveryAddress = RModel.DeliveryAddress.Trim(),
                    AboutMe = RModel.AboutMe.Trim(),
                    PhoneNumber = RModel.MobileNo.Trim(),
                    Photo = fileName,
                    PasswordHistory = protector.Protect(RModel.Password).Trim(),
                };

                var result = await userManager.CreateAsync(user, RModel.Password.Trim());

                if (result.Succeeded)
                {
                    result = await userManager.AddToRoleAsync(user, "Member");

                    // Generate email confirmation token
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Construct the confirmation link
                    var confirmationLink = Url.Page(
                        "/Register",
                        pageHandler: null,
                        values: new { userId = user.Id, code = token },
                        protocol: Request.Scheme);

                    // Send confirmation email
                    await SendConfirmationEmailAsync(user.Email, confirmationLink);

                    // Redirect to the root page with a query parameter
                    return RedirectToPage("/Index", new { emailVerification = true });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }

        private async Task SendConfirmationEmailAsync(string userEmail, string confirmationLink)
        {
            // Use Ethereal Email settings for testing
            var smtpSettings = configuration.GetSection("SmtpSettings");

            var smtpHost = smtpSettings["Host"];
            var smtpPort = 587;
            var smtpUsername = smtpSettings["Username"];
            var smtpPassword = smtpSettings["Password"];
            var senderEmail = userEmail;

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true; // Ethereal does not use SSL

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Confirm your email",
                    Body = $"<a href='{confirmationLink}'>Click here to confirm your email</a>",
                    IsBodyHtml = true
                };

                message.To.Add(userEmail);

                await client.SendMailAsync(message);
            }
        }

    }
}
