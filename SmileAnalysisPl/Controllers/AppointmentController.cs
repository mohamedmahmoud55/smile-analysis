using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.AppointmentViewModels;

namespace SmileAnalysisPl.Controllers;

[Authorize(Roles = "SuperAdmin,Admin,Staff")]
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public IActionResult Index()
    {
        return View(_appointmentService.GetAll());
    }

    public IActionResult Create(int? patientId)
    {
        return View(_appointmentService.GetCreateForm(patientId));
    }

    /// <summary>JSON for booking UI: start times (HH:mm:ss) within doctor availability; optional exclude id for edit.</summary>
    [HttpGet]
    public IActionResult GetAvailableSlots(int doctorId, DateOnly date, int durationMinutes = 30, int? excludeAppointmentId = null)
    {
        if (doctorId <= 0)
            return Json(new { slots = Array.Empty<string>() });
        var slots = _appointmentService.GetAvailableSlotStarts(doctorId, date, durationMinutes, excludeAppointmentId);
        return Json(new { slots = slots.Select(s => s.ToString("HH:mm:ss")).ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            RepopulateSelectLists(model);
            return View(model);
        }

        if (_appointmentService.TryCreate(model, out var err))
        {
            TempData["SuccessMessage"] = "Appointment scheduled.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", err ?? "Could not create appointment.");
        RepopulateSelectLists(model);
        return View(model);
    }

    public IActionResult Edit(int id)
    {
        var model = _appointmentService.GetForEdit(id);
        if (model is null)
        {
            TempData["ErrorMessage"] = "That appointment cannot be edited (missing, completed, or already cancelled).";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            RepopulateEditLists(model);
            return View(model);
        }

        if (_appointmentService.TryUpdate(model, out var err))
        {
            TempData["SuccessMessage"] = "Appointment updated.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", err ?? "Could not update appointment.");
        RepopulateEditLists(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        if (_appointmentService.TryCancel(id, out var err))
            TempData["SuccessMessage"] = "Appointment cancelled.";
        else
            TempData["ErrorMessage"] = err ?? "Could not cancel appointment.";
        return RedirectToAction(nameof(Index));
    }

    private void RepopulateSelectLists(CreateAppointmentViewModel model)
    {
        var form = _appointmentService.GetCreateForm(model.PatientId);
        model.Patients = form.Patients;
        model.Doctors = form.Doctors;
    }

    private void RepopulateEditLists(EditAppointmentViewModel model)
    {
        var src = _appointmentService.GetForEdit(model.Id);
        if (src is null)
            return;
        model.Patients = src.Patients;
        model.Doctors = src.Doctors;
    }
}
