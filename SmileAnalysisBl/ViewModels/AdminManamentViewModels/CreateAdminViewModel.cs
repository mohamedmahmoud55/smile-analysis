using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.AdminManamentViewModels
{
    public class CreateAdminViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
