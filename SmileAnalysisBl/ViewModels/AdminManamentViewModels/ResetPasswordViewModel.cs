using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.AdminManamentViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Id { get; set; } = null!;
         
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
