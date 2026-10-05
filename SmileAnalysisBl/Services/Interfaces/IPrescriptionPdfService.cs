namespace SmileAnalysisBl.Services.Interfaces;

public interface IPrescriptionPdfService
{
    byte[]? GeneratePdf(int prescriptionId);
}
