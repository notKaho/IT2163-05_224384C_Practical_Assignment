using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IT2163_05_224384C.Pages
{
	[Authorize(Policy = "MustBeAMember", AuthenticationSchemes = "MyCookieAuth")]
	public class MembershipModel : PageModel
    {
		private readonly ILogger<MembershipModel> _logger;

		public MembershipModel(ILogger<MembershipModel> logger)
		{
			_logger = logger;
		}

		public void OnGet()
		{
		}
	}
}
