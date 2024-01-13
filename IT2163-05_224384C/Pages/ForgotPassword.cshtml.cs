using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.DataProtection;

namespace IT2163_05_224384C.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Member> userManager;
        private readonly IConfiguration configuration;

        [BindProperty]
        [Required(ErrorMessage = "Email address is required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        public ForgotPasswordModel(UserManager<Member> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public void OnGet()
        {
            // This is the handler for HTTP GET requests to the page.
        }

        public async Task<IActionResult> OnPostAsync()
        {         
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByEmailAsync(Email);

                    if (user != null)
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        var resetLink = Url.Page("/ResetPassword", null, new { area = "Identity", email = Email, token }, Request.Scheme);

                        // Send the reset password link by email
                        await SendForgotPasswordAsync(Email, resetLink);

                        // Provide a user-friendly success message
                        ViewData["EmailSuccess"] = "A reset password link has been sent to your email.";
                        return Page();
                    }
                    else
                    {
                        // Email not found in the Identity system
                        ModelState.AddModelError("Email", "Email address not found.");
                    }
                }

                // If there are validation errors or the email is not found, return to the form
                return Page();
            }
            catch (Exception)
            {
                // Handle exception with a generic error message
                ViewData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return Page();
            }
        }



        private async Task SendForgotPasswordAsync(string userEmail, string resetLink)
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
                    Subject = "Reset your password",
                    Body = $"Click <a href='{resetLink}'>here</a> to reset your password.",
                    IsBodyHtml = true
                };

                message.To.Add(userEmail);

                await client.SendMailAsync(message);
            }
        }
    }


}
