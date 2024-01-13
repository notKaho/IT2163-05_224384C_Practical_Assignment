using System.ComponentModel.DataAnnotations;

namespace IT2163_05_224384C.ViewModels
{
    public class Login
    {
		[Required(ErrorMessage = "Email address is required")]
		[DataType(DataType.EmailAddress)]
		[EmailAddress(ErrorMessage = "Invalid email address")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)]
		[MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
			ErrorMessage = "Password must include at least one lowercase letter, one uppercase letter, one digit, and one special character")]
		public string Password { get; set; }
    }
}
