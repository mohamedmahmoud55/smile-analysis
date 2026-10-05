using Microsoft.AspNetCore.Identity;
using SmileAnalysisBl.ViewModels.AdminManamentViewModels;

namespace SmileAnalysisBl.Services.Interfaces
{
    public interface IAdminService
    {
        IEnumerable<AdminListViewModel> GetAllManagedUsers();
        IdentityResult CreateAdmin(CreateAdminViewModel model);
        EditAdminViewModel GetAdminForUpdate(string id);
        IdentityResult EditAdmin(EditAdminViewModel model);
        IdentityResult DeleteAdmin(string id);
        IdentityResult ResetPassword(ResetPasswordViewModel model);
        ManageRolesViewModel GetRoles(string userId);
        IdentityResult AddRole(string userId, string role);
        IdentityResult RemoveRole(string userId, string role);
    }
}
