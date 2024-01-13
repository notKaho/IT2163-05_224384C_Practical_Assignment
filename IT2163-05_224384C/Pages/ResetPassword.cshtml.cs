using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.DataProtection;

namespace IT2163_05_224384C.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<Member> userManager;
        private readonly IConfiguration configuration;
        private readonly ILogger<ResetPasswordModel> logger;


        [BindProperty(SupportsGet = true)]
        [Required(ErrorMessage = "Email address is required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
            ErrorMessage = "Password must include at least one lowercase letter, one uppercase letter, one digit, and one special character")]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        public ResetPasswordModel(UserManager<Member> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.logger = logger;
        }

        public IActionResult OnGet()
        {
            return Token == null ? RedirectToPage("/Index") : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Email);

                if (user != null)
                {
                    var currentTime = DateTime.UtcNow;

                    // Check minimum password age (e.g., cannot change password within 30 minutes from the last change)
                    var minPasswordAge = TimeSpan.FromMinutes(1); // Adjust the minimum age as needed
                    if ((currentTime - user.LastPasswordChangeDate) < minPasswordAge)
                    {
                        ModelState.AddModelError("MinPasswordAge", "Cannot change password within the minimum password age period.");
                        return Page();
                    }

                    // Verify the new password against the password history
                    var passwordHistoryArray = user.PasswordHistory?.Split(';').ToList() ?? new List<string>();

                    // Unprotect each item in the password history
                    var unprotectedPasswordHistory = passwordHistoryArray.Select(protector.Unprotect).ToList();

                    if (unprotectedPasswordHistory.Contains(Password.Trim()))
                    {
                        ModelState.AddModelError("PasswordHistory", "New password cannot be the same as a previous password.");
                        return Page();
                    }
                    else
                    {
                        // Limit the password history to two passwords
                        if (passwordHistoryArray.Count >= 2)
                        {
                            passwordHistoryArray.RemoveAt(0); // Remove the first index if there are already two passwords
                        }

                        passwordHistoryArray.Add(protector.Protect(Password.Trim())); // Add the new password to the password history

                        var result = await userManager.ResetPasswordAsync(user, Token, Password);

                        if (result.Succeeded)
                        {
                            // Update the PasswordHistory attribute
                            user.PasswordHistory = string.Join(";", passwordHistoryArray);

                            // Password reset successful
                            user.LastPasswordChangeDate = currentTime;
                            await userManager.UpdateAsync(user); // Update the user with the new password history
                            return RedirectToPage("/Login"); // Redirect to login page
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                }
                else
                {
                    // User not found
                    ModelState.AddModelError("Email", "User not found.");
                }
            }

            // If there are validation errors or the user is not found, return to the form
            return Page();
        }
    }
}
