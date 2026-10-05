using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.GummySmile;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.GummySmileViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class GummySmileController : Controller
{
    private readonly IGummySmileCaseService _gummySmileCaseService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GummySmileController(
        IGummySmileCaseService gummySmileCaseService,
        IPatientService patientService,
        IDoctorService doctorService,
        UserManager<ApplicationUser> userManager)
    {
        _gummySmileCaseService = gummySmileCaseService;
        _patientService = patientService;
        _doctorService = doctorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Start(int patientId, CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

        try
        {
            var localCaseId = await _gummySmileCaseService.StartCaseAsync(patientId, cancellationToken);
            return RedirectToAction(nameof(Upload), new { id = localCaseId });
        }
        catch (GummySmileApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Patient", new { id = patientId });
        }
    }

    public async Task<IActionResult> Upload(int id)
    {
        var model = _gummySmileCaseService.GetUploadPage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int id, IFormFile? restImage, IFormFile? smileImage, CancellationToken cancellationToken)
    {
        var model = _gummySmileCaseService.GetUploadPage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return NotFound();

        if (restImage is null || restImage.Length == 0)
            ModelState.AddModelError(nameof(restImage), "Rest (before) image is required.");
        if (smileImage is null || smileImage.Length == 0)
            ModelState.AddModelError(nameof(smileImage), "Smile image is required.");

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await using var restStream = restImage!.OpenReadStream();
            await using var smileStream = smileImage!.OpenReadStream();
            var ok = await _gummySmileCaseService.UploadImagesAsync(
                id, restStream, restImage.FileName, restImage.ContentType,
                smileStream, smileImage.FileName, smileImage.ContentType, cancellationToken);

            if (!ok)
            {
                ModelState.AddModelError("", "Could not upload images.");
                return View(model);
            }

            return RedirectToAction(nameof(Analyze), new { id });
        }
        catch (GummySmileApiException ex)
        {
            model.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    public async Task<IActionResult> Analyze(int id)
    {
        var model = _gummySmileCaseService.GetAnalyzePage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(int id, CancellationToken cancellationToken)
    {
        var model = _gummySmileCaseService.GetAnalyzePage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return NotFound();

        try
        {
            var ok = await _gummySmileCaseService.RunAnalysisAsync(id, cancellationToken);
            if (!ok)
            {
                ModelState.AddModelError("", "Analysis could not be started.");
                return View(model);
            }

            return RedirectToAction(nameof(Review), new { id });
        }
        catch (GummySmileApiException ex)
        {
            model.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    public async Task<IActionResult> Review(int id)
    {
        var model = _gummySmileCaseService.GetReviewPage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return RedirectToAction(nameof(Analyze), new { id });
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(GummySmileReviewViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientAsync(model.PatientId))
            return Forbid();

        try
        {
            await _gummySmileCaseService.SubmitOverridesAsync(model, cancellationToken);
            return RedirectToAction(nameof(Clinical), new { id = model.LocalCaseId });
        }
        catch (GummySmileApiException ex)
        {
            model.ErrorMessage = ex.Message;
            var refreshed = _gummySmileCaseService.GetReviewPage(model.LocalCaseId);
            if (refreshed is not null)
            {
                refreshed.GdOverrides = model.GdOverrides;
                refreshed.ErrorMessage = ex.Message;
                return View(refreshed);
            }

            return View(model);
        }
    }

    public async Task<IActionResult> Clinical(int id)
    {
        var model = _gummySmileCaseService.GetClinicalPage(id);
        if (model is null || !await CanAccessPatientAsync(model.PatientId))
            return RedirectToAction(nameof(Review), new { id });
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveClinicalDraft(GummySmileClinicalViewModel model)
    {
        if (!await CanAccessPatientAsync(model.PatientId))
            return Forbid();

        await _gummySmileCaseService.SaveClinicalDraftAsync(model);
        TempData["SuccessMessage"] = "Clinical draft saved locally.";
        return RedirectToAction(nameof(Clinical), new { id = model.LocalCaseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clinical(GummySmileClinicalViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientAsync(model.PatientId))
            return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var ok = await _gummySmileCaseService.SubmitClinicalAsync(model, cancellationToken);
            if (!ok)
            {
                ModelState.AddModelError("", "Diagnosis is not ready. Verify clinical inputs and try again.");
                return View(model);
            }

            return RedirectToAction(nameof(Diagnosis), new { id = model.LocalCaseId });
        }
        catch (GummySmileApiException ex)
        {
            model.ErrorMessage = ex.Message;
            return View(model);
        }
    }

    public async Task<IActionResult> Diagnosis(int id, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _gummySmileCaseService.GetDiagnosisPageAsync(id, cancellationToken);
            if (model is null || !await CanAccessPatientAsync(model.PatientId))
                return NotFound();
            return View(model);
        }
        catch (GummySmileApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Clinical), new { id });
        }
    }

    public async Task<IActionResult> Treatment(int id, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _gummySmileCaseService.GetTreatmentPageAsync(id, cancellationToken);
            if (model is null || !await CanAccessPatientAsync(model.PatientId))
                return NotFound();
            return View(model);
        }
        catch (GummySmileApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Diagnosis), new { id });
        }
    }

    public IActionResult Results(int id) => RedirectToAction(nameof(Diagnosis), new { id });

    public async Task<IActionResult> Asset(int id, string path, CancellationToken cancellationToken)
    {
        var uploadPage = _gummySmileCaseService.GetUploadPage(id);
        if (uploadPage is null || !await CanAccessPatientAsync(uploadPage.PatientId))
            return NotFound();

        try
        {
            var result = await _gummySmileCaseService.GetAssetAsync(id, path, cancellationToken);
            if (result is null)
                return NotFound();
            return File(result.Value.Data, result.Value.ContentType);
        }
        catch (GummySmileApiException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> DownloadPdf(int id, CancellationToken cancellationToken)
    {
        var uploadPage = _gummySmileCaseService.GetUploadPage(id);
        if (uploadPage is null || !await CanAccessPatientAsync(uploadPage.PatientId))
            return NotFound();

        try
        {
            var result = await _gummySmileCaseService.GetReportPdfAsync(id, cancellationToken);
            if (result is null)
                return NotFound();

            return File(result.Value.Data, "application/pdf", result.Value.FileName);
        }
        catch (GummySmileApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Treatment), new { id });
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
