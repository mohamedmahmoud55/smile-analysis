using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PatientViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class PatientController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(
        IPatientService patientService,
        IDoctorService doctorService,
        UserManager<ApplicationUser> userManager)
    {
        _patientService = patientService;
        _doctorService = doctorService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? q)
    {
        if (User.IsInRole("SuperAdmin"))
            return View(_patientService.GetPatientRecordsPage(q));

        if (User.IsInRole("Doctor"))
        {
            var doctorId = await GetCurrentUserDoctorIdAsync();
            if (doctorId is null)
            {
                TempData["ErrorMessage"] =
                    "No doctor profile is linked to your account. Use the same email on your doctor record as your login.";
                return RedirectToAction("Index", "Home");
            }

            return View(_patientService.GetPatientRecordsPageForDoctor(doctorId.Value, q));
        }

        return View(_patientService.GetPatientRecordsPage(q));
    }

    [Authorize(Roles = "SuperAdmin,Staff")]
    public IActionResult Create()
    {
        return View(new CreatePatientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Staff")]
    public IActionResult Create(CreatePatientViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_patientService.Create(model))
        {
            TempData["SuccessMessage"] = "Patient added.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not save patient.");
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin,Staff,Doctor")]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await CanDoctorOrAdminEditPatientAsync(id))
            return Forbid();

        var model = _patientService.GetForEdit(id);
        if (model is null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Staff,Doctor")]
    public async Task<IActionResult> Edit(EditPatientViewModel model)
    {
        if (!await CanDoctorOrAdminEditPatientAsync(model.Id))
            return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        if (_patientService.Update(model))
        {
            TempData["SuccessMessage"] = "Patient updated.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not update patient.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Staff")]
    public IActionResult Delete(int id)
    {
        if (_patientService.TryDelete(id, out var error))
            TempData["SuccessMessage"] = "Patient removed.";
        else
            TempData["ErrorMessage"] = error ?? "Could not remove patient.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!await CanAccessPatientAsync(id))
            return Forbid();

        var model = _patientService.GetPatientDetails(id);
        if (model is null)
            return NotFound();
        return View(model);
    }

    public async Task<IActionResult> GummySmileAnalysis(int id)
    {
        if (!await CanAccessPatientAsync(id))
            return Forbid();

        return RedirectToAction("Start", "GummySmile", new { patientId = id });
    }

    public IActionResult XRayAnalysis(int id) =>
        RedirectToAction("Analyze", "XRayAnalysis", new { patientId = id });

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

    private async Task<bool> CanDoctorOrAdminEditPatientAsync(int patientId)
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("Staff"))
            return true;

        if (!User.IsInRole("Doctor"))
            return false;

        var doctorId = await GetCurrentUserDoctorIdAsync();
        return doctorId is not null && _patientService.PatientHasAppointmentWithDoctor(patientId, doctorId.Value);
    }
}
