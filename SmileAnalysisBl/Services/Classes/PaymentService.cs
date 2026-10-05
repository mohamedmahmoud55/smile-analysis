using Microsoft.AspNetCore.Mvc.Rendering;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.PaymentViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

internal static class BillingMath
{
    public static decimal GrossTotal(decimal baseTreatment, decimal additional) =>
        baseTreatment + additional;

    public static decimal DiscountAmount(BillingDiscountType type, decimal gross, decimal percent, decimal fixedAmount) =>
        type switch
        {
            BillingDiscountType.Percent => Math.Round(
                gross * (Math.Clamp(percent, 0m, 100m) / 100m), 2, MidpointRounding.AwayFromZero),
            BillingDiscountType.FixedAmount => Math.Min(gross, Math.Max(0m, fixedAmount)),
            _ => 0m
        };

    public static decimal NetDue(decimal gross, decimal discountAmount) =>
        Math.Max(0m, gross - discountAmount);
}

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public PatientBillingPageViewModel GetBillingPage(int? patientId)
    {
        var patients = _unitOfWork.GetRepository<Patient>().GetAll()
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .ToList();

        var patientItems = new List<SelectListItem> { new("-- Select a patient --", "") };
        patientItems.AddRange(patients.Select(p => new SelectListItem(
            $"{p.FirstName} {p.LastName} (P-{p.Id:0000})",
            p.Id.ToString())));

        PatientBillingSummaryViewModel? summary = null;
        if (patientId is > 0 && patients.FirstOrDefault(p => p.Id == patientId.Value) is { } patient)
        {
            var account = _unitOfWork.GetRepository<PatientBillingAccount>().GetAll()
                .FirstOrDefault(a => a.PatientId == patientId.Value);
            var payments = _unitOfWork.GetRepository<Payment>().GetAll()
                .Where(p => p.PatientId == patientId.Value)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .ToList();

            summary = BuildSummary(patient, account, payments);
        }

        return new PatientBillingPageViewModel
        {
            Patients = patientItems,
            SelectedPatientId = patientId,
            Summary = summary
        };
    }

    public bool TrySaveBilling(SavePatientBillingInputModel model, out string? errorMessage)
    {
        errorMessage = null;
        if (!TryMapDiscountMode(model.DiscountMode, out var discountType))
        {
            errorMessage = "Invalid discount mode.";
            return false;
        }

        var patientRepo = _unitOfWork.GetRepository<Patient>();
        if (patientRepo.GetById(model.PatientId) is null)
        {
            errorMessage = "Patient not found.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<PatientBillingAccount>();
        var existing = repo.GetAll().FirstOrDefault(a => a.PatientId == model.PatientId);
        PatientBillingAccount account;
        if (existing is null)
        {
            account = new PatientBillingAccount
            {
                PatientId = model.PatientId,
                CreatedAt = DateTime.UtcNow
            };
            repo.Add(account);
        }
        else
        {
            account = existing;
        }

        account.BaseTreatmentCost = Math.Round(Math.Max(0, model.BaseTreatmentCost), 2, MidpointRounding.AwayFromZero);
        account.DiscountType = discountType;
        account.DiscountPercent = discountType == BillingDiscountType.Percent
            ? Math.Clamp(model.DiscountPercent, 0m, 100m)
            : 0m;
        account.DiscountFixedAmount = discountType == BillingDiscountType.FixedAmount
            ? Math.Round(Math.Max(0, model.DiscountFixedAmount), 2, MidpointRounding.AwayFromZero)
            : 0m;
        account.UpdatedAt = DateTime.UtcNow;

        if (existing is not null)
            repo.Update(account);

        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryAddAdditionalCost(AddAdditionalCostInputModel model, out string? errorMessage)
    {
        errorMessage = null;
        var patientRepo = _unitOfWork.GetRepository<Patient>();
        if (patientRepo.GetById(model.PatientId) is null)
        {
            errorMessage = "Patient not found.";
            return false;
        }

        var repo = _unitOfWork.GetRepository<PatientBillingAccount>();
        var existing = repo.GetAll().FirstOrDefault(a => a.PatientId == model.PatientId);
        PatientBillingAccount account;
        if (existing is null)
        {
            account = new PatientBillingAccount
            {
                PatientId = model.PatientId,
                CreatedAt = DateTime.UtcNow
            };
            repo.Add(account);
        }
        else
        {
            account = existing;
        }

        var add = Math.Round(model.Amount, 2, MidpointRounding.AwayFromZero);
        account.AdditionalCosts = Math.Round(account.AdditionalCosts + add, 2, MidpointRounding.AwayFromZero);
        account.UpdatedAt = DateTime.UtcNow;

        if (existing is not null)
            repo.Update(account);

        return _unitOfWork.SaveChanges() > 0;
    }

    public bool TryRecordPayment(RecordFlexiblePaymentInputModel model, out string? errorMessage, out int newPaymentId)
    {
        errorMessage = null;
        newPaymentId = 0;
        if (_unitOfWork.GetRepository<Patient>().GetById(model.PatientId) is null)
        {
            errorMessage = "Patient not found.";
            return false;
        }

        var amount = Math.Round(model.Amount, 2, MidpointRounding.AwayFromZero);
        if (amount <= 0)
        {
            errorMessage = "Payment amount must be greater than zero.";
            return false;
        }

        var account = _unitOfWork.GetRepository<PatientBillingAccount>().GetAll()
            .FirstOrDefault(a => a.PatientId == model.PatientId);
        var existingPayments = _unitOfWork.GetRepository<Payment>().GetAll()
            .Where(p => p.PatientId == model.PatientId)
            .ToList();
        var remaining = ComputeRemainingBalance(account, existingPayments);

        if (remaining <= 0)
        {
            errorMessage = "This patient has no remaining balance to pay.";
            return false;
        }

        if (amount > remaining)
        {
            errorMessage = $"Payment amount cannot exceed the remaining balance ({remaining:N2} EGP).";
            return false;
        }

        var paymentDate = NormalizePaymentDate(model.PaymentDate);

        var entity = new Payment
        {
            AppointmentId = null,
            PatientId = model.PatientId,
            Amount = amount,
            PaymentDate = paymentDate,
            Method = model.Method,
            Status = PaymentStatus.Paid,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.GetRepository<Payment>().Add(entity);
        if (_unitOfWork.SaveChanges() <= 0)
        {
            errorMessage = "Could not save payment.";
            return false;
        }

        newPaymentId = entity.Id;
        return true;
    }

    public PaymentReceiptViewModel? GetPaymentReceipt(int paymentId)
    {
        var payment = _unitOfWork.GetRepository<Payment>().GetById(paymentId);
        if (payment is null)
            return null;

        var patient = _unitOfWork.GetRepository<Patient>().GetById(payment.PatientId);
        if (patient is null)
            return null;

        var account = _unitOfWork.GetRepository<PatientBillingAccount>().GetAll()
            .FirstOrDefault(a => a.PatientId == payment.PatientId);
        var patientPayments = _unitOfWork.GetRepository<Payment>().GetAll()
            .Where(p => p.PatientId == payment.PatientId)
            .ToList();
        var summary = BuildSummary(patient, account, patientPayments);

        return new PaymentReceiptViewModel
        {
            PaymentId = payment.Id,
            TransactionDisplayId = $"TXN-{payment.Id:00000}",
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
            NetAmountDue = summary.NetAmountDue,
            TotalPaid = summary.TotalPaid,
            RemainingBalance = summary.RemainingBalance,
            IssuedAtUtc = DateTime.UtcNow
        };
    }

    public bool TryResetBillingForNewCycle(int patientId, out string? errorMessage)
    {
        errorMessage = null;
        if (_unitOfWork.GetRepository<Patient>().GetById(patientId) is null)
        {
            errorMessage = "Patient not found.";
            return false;
        }

        var account = _unitOfWork.GetRepository<PatientBillingAccount>().GetAll()
            .FirstOrDefault(a => a.PatientId == patientId);
        var payments = _unitOfWork.GetRepository<Payment>().GetAll()
            .Where(p => p.PatientId == patientId)
            .ToList();

        var remaining = ComputeRemainingBalance(account, payments);
        if (remaining > 0m)
        {
            errorMessage = "Remaining balance must be zero before starting a new billing cycle.";
            return false;
        }

        var paymentRepo = _unitOfWork.GetRepository<Payment>();
        foreach (var p in payments)
            paymentRepo.Delete(p);

        var accountRepo = _unitOfWork.GetRepository<PatientBillingAccount>();
        if (account is not null)
        {
            account.BaseTreatmentCost = 0m;
            account.AdditionalCosts = 0m;
            account.DiscountType = BillingDiscountType.None;
            account.DiscountPercent = 0m;
            account.DiscountFixedAmount = 0m;
            account.UpdatedAt = DateTime.UtcNow;
            accountRepo.Update(account);
        }

        _unitOfWork.SaveChanges();
        return true;
    }

    private static decimal ComputeRemainingBalance(
        PatientBillingAccount? account,
        IReadOnlyList<Payment> payments)
    {
        var baseCost = account?.BaseTreatmentCost ?? 0m;
        var additional = account?.AdditionalCosts ?? 0m;
        var dtype = account?.DiscountType ?? BillingDiscountType.None;
        var pct = account?.DiscountPercent ?? 0m;
        var fix = account?.DiscountFixedAmount ?? 0m;

        var gross = BillingMath.GrossTotal(baseCost, additional);
        var discountAmt = BillingMath.DiscountAmount(dtype, gross, pct, fix);
        var net = BillingMath.NetDue(gross, discountAmt);
        var paid = payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .Sum(p => p.Amount);
        return net - paid;
    }

    private static DateTime NormalizePaymentDate(DateTime paymentDate)
    {
        if (paymentDate.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(paymentDate.Date, DateTimeKind.Utc);
        return paymentDate.Kind == DateTimeKind.Utc ? paymentDate : paymentDate.ToUniversalTime();
    }

    private static PatientBillingSummaryViewModel BuildSummary(
        Patient patient,
        PatientBillingAccount? account,
        IReadOnlyList<Payment> payments)
    {
        var baseCost = account?.BaseTreatmentCost ?? 0m;
        var additional = account?.AdditionalCosts ?? 0m;
        var dtype = account?.DiscountType ?? BillingDiscountType.None;
        var pct = account?.DiscountPercent ?? 0m;
        var fix = account?.DiscountFixedAmount ?? 0m;

        var gross = BillingMath.GrossTotal(baseCost, additional);
        var discountAmt = BillingMath.DiscountAmount(dtype, gross, pct, fix);
        var net = BillingMath.NetDue(gross, discountAmt);
        var paid = payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .Sum(p => p.Amount);
        var remaining = net - paid;

        var history = payments.Select(p => new PaymentHistoryRowViewModel
        {
            PaymentId = p.Id,
            TransactionDisplayId = $"TXN-{p.Id:00000}",
            PaymentDate = p.PaymentDate,
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status
        }).ToList();

        return new PatientBillingSummaryViewModel
        {
            PatientId = patient.Id,
            PatientDisplayName = $"{patient.FirstName} {patient.LastName}".Trim(),
            BaseTreatmentCost = baseCost,
            AdditionalCosts = additional,
            DiscountType = dtype,
            DiscountPercent = pct,
            DiscountFixedAmount = fix,
            GrossTotalCost = gross,
            DiscountAmountApplied = discountAmt,
            NetAmountDue = net,
            TotalPaid = paid,
            RemainingBalance = remaining,
            PaymentHistory = history
        };
    }

    private static bool TryMapDiscountMode(string? mode, out BillingDiscountType discountType)
    {
        discountType = BillingDiscountType.None;
        if (string.IsNullOrWhiteSpace(mode))
            return true;
        return Enum.TryParse(mode.Trim(), true, out discountType);
    }
}
