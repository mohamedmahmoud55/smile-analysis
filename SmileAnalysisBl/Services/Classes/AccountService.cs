using Microsoft.AspNetCore.Identity;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.AccountViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisBl.Services.Classes
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser? ValidateUser(LoginViewModel loginViewModel)
        {
            var user = _userManager.FindByEmailAsync(loginViewModel.Email).Result;

            if (user == null) return null;

            var isPasswordValid = _userManager.CheckPasswordAsync(user, loginViewModel.Password).Result;

            return isPasswordValid ? user : null;

        }
    }
}
