using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmileAnalysisBl.Configuration;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class PrescriptionPdfService : IPrescriptionPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClinicOptions _clinicOptions;

    public PrescriptionPdfService(IUnitOfWork unitOfWork, IOptions<ClinicOptions> clinicOptions)
    {
        _unitOfWork = unitOfWork;
        _clinicOptions = clinicOptions.Value;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[]? GeneratePdf(int prescriptionId)
    {
        var prescription = _unitOfWork.GetRepository<Prescription>().GetById(prescriptionId);
        if (prescription is null)
            return null;

        var appointment = _unitOfWork.GetRepository<Appointment>().GetById(prescription.AppointmentId);
        if (appointment is null)
            return null;

        var patient = _unitOfWork.GetRepository<Patient>().GetById(appointment.PatientId);
        var doctor = _unitOfWork.GetRepository<Doctor>().GetById(appointment.DoctorId);
        if (patient is null || doctor is null)
            return null;

        var items = _unitOfWork.GetRepository<PrescriptionItem>()
            .GetAll(i => i.PrescriptionId == prescriptionId)
            .OrderBy(i => i.Id)
            .ToList();

        var patientName = $"{patient.FirstName} {patient.LastName}".Trim();
        var doctorName = $"Dr. {doctor.FirstName} {doctor.LastName}".Trim();
        var dateDisplay = appointment.AppointmentTime.ToString("MMMM dd, yyyy");
        var clinicName = string.IsNullOrWhiteSpace(_clinicOptions.Name)
            ? "Smile Analysis Dental Clinic"
            : _clinicOptions.Name;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text(clinicName).Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    if (!string.IsNullOrWhiteSpace(_clinicOptions.Address))
                        col.Item().Text(_clinicOptions.Address).FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrWhiteSpace(_clinicOptions.Phone))
                        col.Item().Text($"Tel: {_clinicOptions.Phone}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Item().Text("PRESCRIPTION").Bold().FontSize(16).FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(12).Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text($"Patient: {patientName}").SemiBold();
                            left.Item().Text($"Patient ID: P-{patient.Id:0000}");
                        });
                        row.RelativeItem().Column(right =>
                        {
                            right.Item().AlignRight().Text($"Doctor: {doctorName}").SemiBold();
                            right.Item().AlignRight().Text($"Date: {dateDisplay}");
                        });
                    });

                    col.Item().PaddingTop(20).Text("Medications").SemiBold().FontSize(13);

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Medication").SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Dosage").SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Duration").SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Instructions").SemiBold();
                        });

                        foreach (var item in items)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(item.MedicationName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(item.Dosage);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(item.Duration);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(item.Instructions ?? "—");
                        }
                    });

                    if (!string.IsNullOrWhiteSpace(prescription.Notes))
                    {
                        col.Item().PaddingTop(20).Text("Notes").SemiBold().FontSize(13);
                        col.Item().PaddingTop(4).Text(prescription.Notes);
                    }

                    col.Item().PaddingTop(40).Column(sig =>
                    {
                        sig.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                        sig.Item().PaddingTop(4).Text("Doctor's Signature").FontSize(10).FontColor(Colors.Grey.Darken2);
                        sig.Item().PaddingTop(24).Text(doctorName).SemiBold();
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("MMM dd, yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }
}
