using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.StaffViewModels;

namespace SmileAnalysisPl.Controllers;

[Authorize]
public class StaffController : Controller
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Index(string? q)
    {
        return View(_staffService.GetStaffRegistryPage(q));
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Create()
    {
        return View(new CreateStaffViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Create(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_staffService.Create(model))
        {
            TempData["SuccessMessage"] = "Staff member added.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not save staff member.");
        return View(model);
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Edit(int id)
    {
        var model = _staffService.GetStaffForEdit(id);
        if (model is null)
            return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Edit(EditStaffViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (_staffService.Update(model))
        {
            TempData["SuccessMessage"] = "Staff member updated.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Could not update staff member.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Delete(int id)
    {
        if (_staffService.TryDelete(id, out var error))
            TempData["SuccessMessage"] = "Staff member removed.";
        else
            TempData["ErrorMessage"] = error ?? "Could not remove staff member.";
        return RedirectToAction(nameof(Index));
    }
}
