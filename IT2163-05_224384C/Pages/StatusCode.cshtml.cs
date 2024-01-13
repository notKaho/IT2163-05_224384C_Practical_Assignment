using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IT2163_05_224384C.Pages
{
    public class StatusCodeModel : PageModel
    {
        public string StatusCode { get; set; }
        public string ErrorMsg { get; set; }
        public string StackTrace { get; set; }

        private readonly ILogger<StatusCodeModel> _logger;

        public StatusCodeModel(ILogger<StatusCodeModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet(string statusCode)
        {
            StatusCode = statusCode;

            if (statusCode == "404")
            {
                ErrorMsg = "The requested page was not found.";
            }
            else if (statusCode == "403")
            {
                ErrorMsg = "The requested page is restricted.";
            }
            else
            {
                ErrorMsg = "An unexpected error occurred.";
            }

            // Get the exception, if available
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature?.Error != null)
            {
                StackTrace = exceptionFeature.Error.StackTrace;
            }

            return Page();
        }
    }
}
