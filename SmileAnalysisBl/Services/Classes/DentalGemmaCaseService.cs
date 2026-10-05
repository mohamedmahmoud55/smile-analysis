using SmileAnalysisBl.DentalGemma;
using SmileAnalysisBl.DentalGemma.Models;
using SmileAnalysisBl.ViewModels.DentalGemmaViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public interface IDentalGemmaCaseService
{
    DentalGemmaPageViewModel GetPage(int? patientId = null);
    Task<DentalGemmaPageViewModel> InvokeChatAsync(
        DentalGemmaPageViewModel model,
        CancellationToken cancellationToken = default);
}

public class DentalGemmaCaseService : IDentalGemmaCaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDentalGemmaService _dentalGemma;

    public DentalGemmaCaseService(IUnitOfWork unitOfWork, IDentalGemmaService dentalGemma)
    {
        _unitOfWork = unitOfWork;
        _dentalGemma = dentalGemma;
    }

    public DentalGemmaPageViewModel GetPage(int? patientId = null)
    {
        var viewModel = new DentalGemmaPageViewModel { PatientId = patientId };

        if (patientId is not null)
        {
            var patient = _unitOfWork.GetRepository<Patient>().GetById(patientId.Value);
            if (patient is not null)
                viewModel.PatientName = $"{patient.FirstName} {patient.LastName}".Trim();
        }

        return viewModel;
    }

    public async Task<DentalGemmaPageViewModel> InvokeChatAsync(
        DentalGemmaPageViewModel model,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?> caseContext = new();

        if (model.PatientId is not null)
        {
            var patient = _unitOfWork.GetRepository<Patient>().GetById(model.PatientId.Value);
            if (patient is not null)
            {
                caseContext["age"] = CalculateAge(patient.DateOfBirth);
                caseContext["sex"] = patient.Gender == Gender.Male ? "male" : "female";
            }
        }

        var payload = new DgGeneralChatRequest
        {
            Message = model.GeneralChatMessage,
            CaseContext = caseContext,
            SafetyProfile = "educational"
        };

        model.LastResult = await _dentalGemma.InvokeAsync(
            DentalGemmaMode.GeneralChat,
            payload,
            cancellationToken);

        return model;
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
