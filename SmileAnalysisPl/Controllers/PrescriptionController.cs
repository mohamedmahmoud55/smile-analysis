using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PrescriptionViewModels;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class PrescriptionController : Controller
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IPrescriptionPdfService _prescriptionPdfService;
    private readonly IDoctorService _doctorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrescriptionController(
        IPrescriptionService prescriptionService,
        IPrescriptionPdfService prescriptionPdfService,
        IDoctorService doctorService,
        UserManager<ApplicationUser> userManager)
    {
        _prescriptionService = prescriptionService;
        _prescriptionPdfService = prescriptionPdfService;
        _doctorService = doctorService;
        _userManager = userManager;
    }

    [Authorize(Roles = "Doctor,SuperAdmin")]
    public async Task<IActionResult> Create(int appointmentId)
    {
        if (_prescriptionService.HasPrescriptionForAppointment(appointmentId))
            return RedirectToAction(nameof(Edit), new { appointmentId });

        var model = _prescriptionService.GetCreateForm(appointmentId);
        if (model is null)
            return NotFound();

        if (!await CanManageAppointmentAsync(appointmentId))
            return Forbid();

        return View("Form", model);
    }

    [Authorize(Roles = "Doctor,SuperAdmin")]
    public async Task<IActionResult> Edit(int appointmentId)
    {
        var model = _prescriptionService.GetEditForm(appointmentId);
        if (model is null)
            return RedirectToAction(nameof(Create), new { appointmentId });

        if (!await CanManageAppointmentAsync(appointmentId))
            return Forbid();

        return View("Form", model);
    }

    public async Task<IActionResult> GetByAppointmentId(int appointmentId)
    {
        var (model, error) = await LoadPrescriptionDetailsAsync(appointmentId);
        if (error is not null)
            return error;

        if (model is null)
            return NotFound();

        return View("Details", model);
    }

    [Authorize(Roles = "Doctor,SuperAdmin,Admin,Staff")]
    public async Task<IActionResult> Details(int appointmentId)
    {
        var (model, error) = await LoadPrescriptionDetailsAsync(appointmentId);
        if (error is not null)
            return error;

        if (model is null)
        {
            TempData["ErrorMessage"] = "No prescription found for this appointment.";
            return RedirectToAction("Index", "Patient");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,SuperAdmin")]
    public async Task<IActionResult> Create(PrescriptionFormViewModel model)
    {
        if (!await CanManageAppointmentAsync(model.AppointmentId))
            return Forbid();

        model.Items = model.Items?
            .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName)
                        || !string.IsNullOrWhiteSpace(i.Dosage)
                        || !string.IsNullOrWhiteSpace(i.Duration))
            .ToList() ?? [];

        if (model.Items.Count == 0)
            ModelState.AddModelError("", "Add at least one medication.");

        if (!ModelState.IsValid)
            return View("Form", model);

        var (doctorId, isSuperAdmin) = await GetDoctorContextAsync();
        if (_prescriptionService.TryCreate(model, doctorId, isSuperAdmin, out var err))
        {
            TempData["SuccessMessage"] = "Prescription saved.";
            return RedirectToAction(nameof(Details), new { appointmentId = model.AppointmentId });
        }

        ModelState.AddModelError("", err ?? "Could not save prescription.");
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,SuperAdmin")]
    public async Task<IActionResult> Edit(PrescriptionFormViewModel model)
    {
        if (!await CanManageAppointmentAsync(model.AppointmentId))
            return Forbid();

        model.IsEdit = true;
        model.Items = model.Items?
            .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName)
                        || !string.IsNullOrWhiteSpace(i.Dosage)
                        || !string.IsNullOrWhiteSpace(i.Duration))
            .ToList() ?? [];

        if (model.Items.Count == 0)
            ModelState.AddModelError("", "Add at least one medication.");

        if (!ModelState.IsValid)
            return View("Form", model);

        var (doctorId, isSuperAdmin) = await GetDoctorContextAsync();
        if (_prescriptionService.TryUpdate(model, doctorId, isSuperAdmin, out var err))
        {
            TempData["SuccessMessage"] = "Prescription updated.";
            return RedirectToAction(nameof(Details), new { appointmentId = model.AppointmentId });
        }

        ModelState.AddModelError("", err ?? "Could not update prescription.");
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor,SuperAdmin")]
    public async Task<IActionResult> Delete(int id, int appointmentId)
    {
        if (!await CanManageAppointmentAsync(appointmentId))
            return Forbid();

        var (doctorId, isSuperAdmin) = await GetDoctorContextAsync();
        var patientId = _prescriptionService.GetById(id)?.PatientId;

        if (_prescriptionService.TryDelete(id, doctorId, isSuperAdmin, out var err))
            TempData["SuccessMessage"] = "Prescription removed.";
        else
            TempData["ErrorMessage"] = err ?? "Could not remove prescription.";

        if (patientId is > 0)
            return RedirectToAction("Details", "Patient", new { id = patientId });

        return RedirectToAction("Index", "Patient");
    }

    [Authorize(Roles = "Doctor,SuperAdmin,Admin,Staff")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var prescription = _prescriptionService.GetById(id);
        if (prescription is null)
            return NotFound();

        if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
        {
            if (!await CanManageAppointmentAsync(prescription.AppointmentId))
                return Forbid();
        }

        var pdf = _prescriptionPdfService.GeneratePdf(id);
        if (pdf is null)
            return NotFound();

        var fileName = $"prescription-{prescription.PatientName.Replace(' ', '-')}-{DateTime.UtcNow:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    private async Task<(PrescriptionDetailsViewModel? Model, IActionResult? Error)> LoadPrescriptionDetailsAsync(int appointmentId)
    {
        if (!await CanViewAppointmentAsync(appointmentId))
            return (null, Forbid());

        return (_prescriptionService.GetByAppointmentId(appointmentId), null);
    }

    private async Task<(int? DoctorId, bool IsSuperAdmin)> GetDoctorContextAsync()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var doctorId = await GetCurrentDoctorIdAsync();
        return (doctorId, isSuperAdmin);
    }

    private async Task<int?> GetCurrentDoctorIdAsync()
    {
        var appUser = await _userManager.GetUserAsync(User);
        if (appUser is null)
            return null;
        return _doctorService.GetDoctorIdForApplicationUser(appUser.Id, appUser.Email);
    }

    private async Task<bool> CanManageAppointmentAsync(int appointmentId)
    {
        if (User.IsInRole("SuperAdmin"))
            return _prescriptionService.AppointmentExists(appointmentId);

        var doctorId = await GetCurrentDoctorIdAsync();
        return _prescriptionService.DoctorCanManageAppointment(doctorId, appointmentId);
    }

    private async Task<bool> CanViewAppointmentAsync(int appointmentId)
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("Staff"))
            return _prescriptionService.AppointmentExists(appointmentId);

        return await CanManageAppointmentAsync(appointmentId);
    }
}
