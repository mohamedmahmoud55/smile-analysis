using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IAnalyticsService analyticsService, UserManager<ApplicationUser> userManager)
        {
            _analyticsService = analyticsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Doctor") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
                return RedirectToAction("Index", "Patient");
            if (User.IsInRole("Staff") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Admin") && !User.IsInRole("Doctor"))
                return RedirectToAction("Index", "Patient");

            var data = _analyticsService.GetOrthoDashboard();
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser != null)
            {
                var name = $"{appUser.FirstName} {appUser.LastName}".Trim();
                data.WelcomeName = string.IsNullOrEmpty(name)
                    ? appUser.UserName ?? appUser.Email ?? "Doctor"
                    : name;
            }

            return View(data);
        }
    }
}
