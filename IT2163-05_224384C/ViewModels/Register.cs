using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FreshFarmMarket.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "Full Name is required")]
        [MinLength(3, ErrorMessage = "Full Name must be at least 3 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Credit Card No is required")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit Card must be 16 digits")]
        public string CreditCard { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Mobile No is required")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile No must be 8 digits")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Delivery Address is required")]
        [RegularExpression(@"^[a-zA-Z0-9\s\.,#-]+$", ErrorMessage = "Invalid characters in the Delivery Address")]
        [MinLength(5, ErrorMessage = "Delivery Address must be at least 5 characters")]
        public string DeliveryAddress { get; set; }


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

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Photo is required")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".jpg" }, ErrorMessage = "Only JPG files are allowed.")]
        public IFormFile Photo { get; set; }

        [Required(ErrorMessage = "About Me is required")]
        [MinLength(3, ErrorMessage = "About Me must be at least 3 characters")]
        public string AboutMe { get; set; }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);

                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Allowed file extensions are: {string.Join(", ", _extensions)}";
        }
    }
}
