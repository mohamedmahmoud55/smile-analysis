using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisBl.ViewModels.PaymentViewModels;

public class PatientBillingPageViewModel
{
    public IEnumerable<SelectListItem> Patients { get; set; } = [];

    public int? SelectedPatientId { get; set; }

    public PatientBillingSummaryViewModel? Summary { get; set; }
}

public class PatientBillingSummaryViewModel
{
    public int PatientId { get; set; }
    public string PatientDisplayName { get; set; } = string.Empty;

    public decimal BaseTreatmentCost { get; set; }
    public decimal AdditionalCosts { get; set; }

    public BillingDiscountType DiscountType { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountFixedAmount { get; set; }

    /// <summary>Base + additional (before discount).</summary>
    public decimal GrossTotalCost { get; set; }

    public decimal DiscountAmountApplied { get; set; }

    /// <summary>Amount owed after discount.</summary>
    public decimal NetAmountDue { get; set; }

    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }

    public IReadOnlyList<PaymentHistoryRowViewModel> PaymentHistory { get; set; } = [];

    /// <summary>True when balance is fully settled and there is billing or payment data to clear for a new cycle.</summary>
    public bool CanResetForNewCycle =>
        RemainingBalance <= 0m &&
        (GrossTotalCost > 0m || PaymentHistory.Count > 0);
}

public class PaymentHistoryRowViewModel
{
    public int PaymentId { get; set; }
    public string TransactionDisplayId { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
}

public class SavePatientBillingInputModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Range(0, 999_999_999.99)]
    public decimal BaseTreatmentCost { get; set; }

    /// <summary>Form values: None, Percent, FixedAmount.</summary>
    [Required]
    public string DiscountMode { get; set; } = "None";

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    [Range(0, 999_999_999.99)]
    public decimal DiscountFixedAmount { get; set; }
}

public class AddAdditionalCostInputModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [Range(0.01, 999_999_999.99)]
    public decimal Amount { get; set; }
}

public class RecordFlexiblePaymentInputModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [Range(0.01, 999_999_999.99)]
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.Card;

    [Required]
    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.Today;
}

public class ResetBillingInputModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }
}

public class PaymentReceiptViewModel
{
    public int PaymentId { get; set; }
    public string TransactionDisplayId { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }

    /// <summary>Current account totals after this payment (same as billing page).</summary>
    public decimal NetAmountDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }

    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public string ClinicName { get; set; } = "Clinical Cobalt";
    public string ClinicTagline { get; set; } = "Precision Orthodontics";
}
