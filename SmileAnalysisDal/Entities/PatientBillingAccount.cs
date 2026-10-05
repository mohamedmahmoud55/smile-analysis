using SmileAnalysisDal.Entities.Enums;

namespace SmileAnalysisDal.Entities;

/// <summary>Per-patient flexible billing: base cost, add-ons, discount rules (no fixed payment schedule).</summary>
public class PatientBillingAccount : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public decimal BaseTreatmentCost { get; set; }

    /// <summary>Cumulative extra charges (lab, materials, etc.).</summary>
    public decimal AdditionalCosts { get; set; }

    public BillingDiscountType DiscountType { get; set; }

    /// <summary>When <see cref="DiscountType"/> is <see cref="BillingDiscountType.Percent"/>, 0–100.</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>When <see cref="DiscountType"/> is <see cref="BillingDiscountType.FixedAmount"/>.</summary>
    public decimal DiscountFixedAmount { get; set; }
}
