using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PaymentViewModels;

namespace SmileAnalysisPl.Controllers;

[Authorize(Roles = "SuperAdmin,Admin,Staff")]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public IActionResult Index(int? patientId)
    {
        return View(_paymentService.GetBillingPage(patientId));
    }

    [HttpGet]
    public IActionResult Receipt(int id)
    {
        var vm = _paymentService.GetPaymentReceipt(id);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveBilling(SavePatientBillingInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
        }

        if (_paymentService.TrySaveBilling(model, out var err))
            TempData["SuccessMessage"] = "Billing details saved. Totals updated.";
        else
            TempData["ErrorMessage"] = err ?? "Could not save billing details.";

        return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddAdditionalCost(AddAdditionalCostInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
        }

        if (_paymentService.TryAddAdditionalCost(model, out var err))
        {
            TempData["SuccessMessage"] =
                $"Added {model.Amount.ToString("N2", CultureInfo.InvariantCulture)} EGP to total cost.";
        }
        else
            TempData["ErrorMessage"] = err ?? "Could not add additional cost.";

        return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecordPayment(RecordFlexiblePaymentInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
        }

        if (_paymentService.TryRecordPayment(model, out var err, out var newPaymentId))
            return RedirectToAction(nameof(Receipt), new { id = newPaymentId });

        TempData["ErrorMessage"] = err ?? "Could not record payment.";
        return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetBillingForNewCycle(ResetBillingInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
        }

        if (_paymentService.TryResetBillingForNewCycle(model.PatientId, out var err))
            TempData["SuccessMessage"] = "Billing and payment history cleared. You can enter a new treatment cost and record new payments.";
        else
            TempData["ErrorMessage"] = err ?? "Could not reset billing.";

        return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
    }
}
