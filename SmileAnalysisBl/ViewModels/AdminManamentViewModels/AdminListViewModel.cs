namespace SmileAnalysisBl.ViewModels.AdminManamentViewModels
{
    public class AdminListViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>Comma-separated role names for display.</summary>
        public string Roles { get; set; } = string.Empty;
    }

}
