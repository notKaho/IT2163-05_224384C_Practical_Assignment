using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IT2163_05_224384C.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<Member> signInManager;
		public LogoutModel(SignInManager<Member> signInManager)
		{
			this.signInManager = signInManager;
		}
		public void OnGet() { }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
			HttpContext.Session.Clear();
            Response.Cookies.Delete("MyCookieAuth");
            await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
