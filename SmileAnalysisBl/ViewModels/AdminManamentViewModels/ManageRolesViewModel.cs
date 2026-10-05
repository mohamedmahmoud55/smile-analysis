namespace SmileAnalysisBl.ViewModels.AdminManamentViewModels
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;

        public List<string> UserRoles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();
    }
}
