using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.AdminManamentViewModels
{
    public class EditAdminViewModel
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; } = null!;
    }
}
