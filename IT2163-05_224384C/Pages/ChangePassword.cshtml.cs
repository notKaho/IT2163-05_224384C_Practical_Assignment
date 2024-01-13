using IT2163_05_224384C.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.AspNetCore.DataProtection;

namespace IT2163_05_224384C.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<Member> userManager;

        [BindProperty]
        public ChangePassword LModel { get; set; }

        public ChangePasswordModel(UserManager<Member> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync()
        {
            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");

            if (ModelState.IsValid)
            {
                var email = HttpContext.Session.GetString("Email");

                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user != null)
                    {
                        var currentTime = DateTime.UtcNow;

                        // Check minimum password age (e.g., cannot change password within 30 minutes from the last change)
                        var minPasswordAge = TimeSpan.FromMinutes(1);
                        if ((currentTime - user.LastPasswordChangeDate) < minPasswordAge)
                        {
                            ModelState.AddModelError("MinPasswordAge", "Cannot change password within the minimum password age period.");
                            return Page();
                        }


                        // Verify the user's identity using their current password
                        var passwordCheckResult = await userManager.CheckPasswordAsync(user, LModel.CurrentPassword);

                        if (passwordCheckResult)
                        {
                            // Retrieve existing password history
                            var passwordHistoryArray = user.PasswordHistory?.Split(';').ToList() ?? new List<string>();

                            // Unprotect each item in the password history
                            var unprotectedPasswordHistory = passwordHistoryArray.Select(protector.Unprotect).ToList();

                            // Check if the new password matches any password in the history
                            if (unprotectedPasswordHistory.Contains(LModel.Password.Trim()))
                            {
                                // New password matches a password in the history
                                ModelState.AddModelError("PasswordHistory", "New password cannot be the same as a previous password.");
                                return Page();
                            }
                            else
                            {
                                // Limit the password history to two passwords
                                if (passwordHistoryArray.Count >= 2)
                                {
                                    // Remove the first index if there are already two passwords
                                    passwordHistoryArray.RemoveAt(0);
                                }

                                // Add the new password to the password history
                                passwordHistoryArray.Add(protector.Protect(LModel.Password.Trim())); // Protect the new password


                                var changePasswordResult = await userManager.ChangePasswordAsync(user, LModel.CurrentPassword, LModel.Password);

                                if (changePasswordResult.Succeeded)
                                {
                                    // Update the PasswordHistory attribute
                                    user.PasswordHistory = string.Join(";", passwordHistoryArray);

                                    // Password changed successfully
                                    user.LastPasswordChangeDate = currentTime;

                                    await userManager.UpdateAsync(user); // Update the user with the new password history
                                    return RedirectToPage("/Index"); // Redirect to a success page
                                }
                            }
                        }
                        else
                        {
                            // Password verification failed
                            ModelState.AddModelError("IncorrectCurrentPassword", "Current password is incorrect.");
                        }
                    }
                }
            }

            // If ModelState is not valid or any other error occurs, show the form again
            return Page();
        }
    }
}
