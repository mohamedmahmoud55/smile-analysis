using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.DentalGemma;
using SmileAnalysisBl.Services.Classes;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.DentalGemmaViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class DentalGemmaController : Controller
{
    private readonly IDentalGemmaCaseService _dentalGemmaCaseService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DentalGemmaController(
        IDentalGemmaCaseService dentalGemmaCaseService,
        IPatientService patientService,
        IDoctorService doctorService,
        UserManager<ApplicationUser> userManager)
    {
        _dentalGemmaCaseService = dentalGemmaCaseService;
        _patientService = patientService;
        _doctorService = doctorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? patientId)
    {
        if (patientId is not null && !await CanAccessPatientAsync(patientId.Value))
            return Forbid();

        return View(_dentalGemmaCaseService.GetPage(patientId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invoke(DentalGemmaPageViewModel model, CancellationToken cancellationToken)
    {
        if (model.PatientId is not null && !await CanAccessPatientAsync(model.PatientId.Value))
            return Forbid();

        if (string.IsNullOrWhiteSpace(model.GeneralChatMessage))
        {
            model.ErrorMessage = "Enter a message for DentalGemma.";
            if (model.PatientId is not null)
                model.PatientName = _dentalGemmaCaseService.GetPage(model.PatientId).PatientName;
            return View("Index", model);
        }

        try
        {
            model = await _dentalGemmaCaseService.InvokeChatAsync(model, cancellationToken);
            return View("Index", model);
        }
        catch (Exception ex)
        {
            model.ErrorMessage = ex switch
            {
                DentalGemmaApiException apiEx => apiEx.Message,
                TaskCanceledException or OperationCanceledException =>
                    "DentalGemma request timed out. Please try again.",
                _ => $"DentalGemma request failed: {ex.Message}"
            };
            if (model.PatientId is not null && string.IsNullOrEmpty(model.PatientName))
                model.PatientName = _dentalGemmaCaseService.GetPage(model.PatientId).PatientName;
            return View("Index", model);
        }
    }

    private async Task<int?> GetCurrentUserDoctorIdAsync()
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser is null)
            return null;
        return _doctorService.GetDoctorIdForApplicationUser(appUser.Id, appUser.Email);
    }

    private async Task<bool> CanAccessPatientAsync(int patientId)
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("Staff") || User.IsInRole("Admin"))
            return true;

        if (!User.IsInRole("Doctor"))
            return true;

        var doctorId = await GetCurrentUserDoctorIdAsync();
        return doctorId is not null && _patientService.PatientHasAppointmentWithDoctor(patientId, doctorId.Value);
    }
}
