using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.DoctorViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController(
        IDoctorService doctorService,
        IAppointmentService appointmentService,
        UserManager<ApplicationUser> userManager)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _userManager = userManager;
    }

    /// <summary>Redirects to <see cref="Schedule"/> for the doctor linked to the current user (by FK or matching email).</summary>
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> MySchedule(DateOnly? start, DateOnly? end)
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser is null)
            return Challenge();

        var doctorId = _doctorService.GetDoctorIdForApplicationUser(appUser.Id, appUser.Email);
        if (doctorId is null)
        {
            TempData["ErrorMessage"] =
                "No doctor profile is linked to your account. Your doctor record email must match your login email, or an administrator can update the doctor profile.";
            return RedirectToAction("Index", "Patient");
        }

        return RedirectToAction(nameof(Schedule), new { id = doctorId.Value, start, end });
    }

    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    public IActionResult SchedulePicker()
    {
        return View(_doctorService.GetDoctorSchedulePicker());
    }

    /// <summary>Appointments assigned to this doctor (date range optional).</summary>
    [Authorize(Roles = "SuperAdmin,Admin,Staff,Doctor")]
    public IActionResult Schedule(int id, DateOnly? start, DateOnly? end)
    {
        var page = _appointmentService.GetDoctorSchedule(id, start, end);
        if (page is null)
            return NotFound();
        return View(page);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Index(string? q)
    {
        return View(_doctorService.GetMedicalTeamPage(q));
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Create()
    {
        return View(new CreateDoctorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Create(CreateDoctorViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_doctorService.Create(model))
        {
            var hasSlots = model.AvailabilitySlots?.Any(s => !s.IsEmpty() && !s.IsPartiallyFilled()) == true;
            TempData["SuccessMessage"] = hasSlots
                ? "Doctor added with weekly availability."
                : "Doctor added.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not save doctor.");
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Edit(int id)
    {
        var model = _doctorService.GetDoctorForEdit(id);
        if (model is null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Edit(EditDoctorViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_doctorService.Update(model))
        {
            TempData["SuccessMessage"] = "Doctor updated.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not update doctor.");
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Availability(int id)
    {
        var page = _doctorService.GetAvailabilityPage(id);
        if (page is null)
            return NotFound();
        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult AddAvailability(AddDoctorAvailabilityViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var pageInvalid = _doctorService.GetAvailabilityPage(model.DoctorId);
            if (pageInvalid is null)
                return NotFound();
            pageInvalid.AddForm = model;
            return View("Availability", pageInvalid);
        }

        if (!_doctorService.TryAddAvailability(model, out var err))
        {
            var page = _doctorService.GetAvailabilityPage(model.DoctorId);
            if (page is null)
                return NotFound();
            page.AddForm = model;
            ModelState.AddModelError("", err ?? "Could not add availability.");
            return View("Availability", page);
        }

        TempData["SuccessMessage"] = "Availability added.";
        return RedirectToAction(nameof(Availability), new { id = model.DoctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult DeleteAvailability(int doctorId, int availabilityId)
    {
        if (!_doctorService.TryDeleteAvailability(doctorId, availabilityId, out var error))
            TempData["ErrorMessage"] = error ?? "Could not remove availability.";
        else
            TempData["SuccessMessage"] = "Availability removed.";
        return RedirectToAction(nameof(Availability), new { id = doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Delete(int id)
    {
        if (_doctorService.TryDelete(id, out var error))
            TempData["SuccessMessage"] = "Doctor removed.";
        else
            TempData["ErrorMessage"] = error ?? "Could not remove doctor.";
        return RedirectToAction(nameof(Index));
    }
}
