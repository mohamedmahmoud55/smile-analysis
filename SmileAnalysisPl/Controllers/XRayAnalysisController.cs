using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.DentalGemma;
using SmileAnalysisBl.XRayAnalysis;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class XRayAnalysisController : Controller
{
    private readonly IXRayAnalysisCaseService _xrayCaseService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public XRayAnalysisController(
        IXRayAnalysisCaseService xrayCaseService,
        IPatientService patientService,
        IDoctorService doctorService,
        UserManager<ApplicationUser> userManager)
    {
        _xrayCaseService = xrayCaseService;
        _patientService = patientService;
        _doctorService = doctorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Analyze(int patientId, int? caseId)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

        var model = _xrayCaseService.GetAnalysisPage(patientId, caseId);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> AnalyzePanoramic(
        int patientId,
        int? caseId,
        IFormFile panoramicImage,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

        var model = _xrayCaseService.GetAnalysisPage(patientId, caseId);
        if (model is null)
            return NotFound();

        if (panoramicImage is null || panoramicImage.Length == 0)
        {
            model.ErrorMessage = "Upload a panoramic (OPG) image.";
            return View("Analyze", model);
        }

        try
        {
            await using var stream = panoramicImage.OpenReadStream();
            var resultCaseId = await _xrayCaseService.RunPanoramicAnalysisAsync(
                patientId,
                caseId,
                stream,
                panoramicImage.FileName,
                panoramicImage.ContentType ?? "image/jpeg",
                cancellationToken);

            return RedirectToAction(nameof(Analyze), new { patientId, caseId = resultCaseId });
        }
        catch (Exception ex)
        {
            return AnalysisErrorView(patientId, caseId, ex);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    [RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> AnalyzeCephalogram(
        int patientId,
        int? caseId,
        IFormFile cephImage,
        bool RunDentalGemma = false,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

        var model = _xrayCaseService.GetAnalysisPage(patientId, caseId);
        if (model is null)
            return NotFound();

        model.RunDentalGemma = RunDentalGemma;

        if (cephImage is null || cephImage.Length == 0)
        {
            model.ErrorMessage = "Upload a lateral cephalogram image.";
            return View("Analyze", model);
        }

        try
        {
            await using var stream = cephImage.OpenReadStream();
            var resultCaseId = await _xrayCaseService.RunCephalogramAnalysisAsync(
                patientId,
                caseId,
                stream,
                cephImage.FileName,
                cephImage.ContentType ?? "image/jpeg",
                model.RunDentalGemma,
                cancellationToken);

            return RedirectToAction(nameof(Analyze), new { patientId, caseId = resultCaseId });
        }
        catch (Exception ex)
        {
            return AnalysisErrorView(patientId, caseId, ex, RunDentalGemma);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SynthesizeFullCase(
        int patientId,
        int caseId,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

        try
        {
            await _xrayCaseService.RunFullCaseSynthesisAsync(patientId, caseId, cancellationToken);
            return RedirectToAction(nameof(Analyze), new { patientId, caseId });
        }
        catch (Exception ex)
        {
            return AnalysisErrorView(patientId, caseId, ex);
        }
    }

    private IActionResult AnalysisErrorView(int patientId, int? caseId, Exception ex, bool runDentalGemma = false)
    {
        var model = _xrayCaseService.GetAnalysisPage(patientId, caseId)
            ?? new XRayAnalysisPageViewModel { PatientId = patientId };

        model.RunDentalGemma = runDentalGemma;
        model.ErrorMessage = ex switch
        {
            XRayAnalysisApiException apiEx => apiEx.Message,
            DentalGemmaApiException dgEx => dgEx.Message,
            TaskCanceledException or OperationCanceledException =>
                "The analysis timed out. The AI service may still be processing — wait a moment and try again.",
            _ => $"Analysis failed: {ex.Message}"
        };

        return View("Analyze", model);
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
