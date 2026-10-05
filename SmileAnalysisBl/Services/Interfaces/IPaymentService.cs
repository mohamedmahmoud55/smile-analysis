using SmileAnalysisBl.ViewModels.PaymentViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IPaymentService
{
    PatientBillingPageViewModel GetBillingPage(int? patientId);

    bool TrySaveBilling(SavePatientBillingInputModel model, out string? errorMessage);

    bool TryAddAdditionalCost(AddAdditionalCostInputModel model, out string? errorMessage);

    bool TryRecordPayment(RecordFlexiblePaymentInputModel model, out string? errorMessage, out int newPaymentId);

    PaymentReceiptViewModel? GetPaymentReceipt(int paymentId);

    /// <summary>Clears payment history and billing totals when remaining balance is zero (or credit). For a new treatment cycle.</summary>
    bool TryResetBillingForNewCycle(int patientId, out string? errorMessage);
}
