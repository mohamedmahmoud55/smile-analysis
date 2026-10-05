using SmileAnalysisBl.ViewModels.AccountViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisBl.Services.Interfaces
{
    public interface IAccountService
    {
        ApplicationUser? ValidateUser(LoginViewModel loginViewModel);
    }
}
