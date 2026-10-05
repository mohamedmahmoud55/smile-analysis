using Microsoft.AspNetCore.Identity;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.AdminManamentViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisBl.Services.Classes;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IEnumerable<AdminListViewModel> GetAllManagedUsers()
    {
        var users = _userManager.Users
            .OrderBy(u => u.Email ?? u.UserName)
            .ToList();

        return users.Select(u =>
        {
            var roles = _userManager.GetRolesAsync(u).Result.OrderBy(r => r).ToList();
            return new AdminListViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                Roles = roles.Count > 0 ? string.Join(", ", roles) : "—"
            };
        });
    }

    public IdentityResult CreateAdmin(CreateAdminViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber
        };

        var result = _userManager.CreateAsync(user, model.Password).Result;

        if (result.Succeeded)
            _userManager.AddToRoleAsync(user, "Admin").Wait();

        return result;
    }

    public EditAdminViewModel GetAdminForUpdate(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null!;

        var user = _userManager.FindByIdAsync(id).Result;

        if (user == null)
            return null!;

        return new EditAdminViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty
        };
    }

    public IdentityResult EditAdmin(EditAdminViewModel model)
    {
        var user = _userManager.FindByIdAsync(model.Id).Result;
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        user.Email = model.Email;
        user.UserName = model.UserName;

        return _userManager.UpdateAsync(user).Result;
    }

    public IdentityResult DeleteAdmin(string id)
    {
        var user = _userManager.FindByIdAsync(id).Result;
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return _userManager.DeleteAsync(user).Result;
    }

    public IdentityResult ResetPassword(ResetPasswordViewModel model)
    {
        var user = _userManager.FindByIdAsync(model.Id).Result;
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;
        return _userManager.ResetPasswordAsync(user, token, model.NewPassword).Result;
    }

    public ManageRolesViewModel GetRoles(string userId)
    {
        var user = _userManager.FindByIdAsync(userId).Result;
        if (user == null)
            return null!;

        var allRoleNames = _roleManager.Roles
            .Select(r => r.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .ToList();
        var userRoles = _userManager.GetRolesAsync(user).Result.ToList();

        return new ManageRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            AllRoles = allRoleNames,
            UserRoles = userRoles
        };
    }

    public IdentityResult AddRole(string userId, string role)
    {
        var user = _userManager.FindByIdAsync(userId).Result;
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return _userManager.AddToRoleAsync(user, role).Result;
    }

    public IdentityResult RemoveRole(string userId, string role)
    {
        var user = _userManager.FindByIdAsync(userId).Result;
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return _userManager.RemoveFromRoleAsync(user, role).Result;
    }
}
