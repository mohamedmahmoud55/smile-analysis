using SmileAnalysisBl.ViewModels.StaffViewModels;

namespace SmileAnalysisBl.Services.Interfaces;

public interface IStaffService
{
    StaffRecordsPageViewModel GetStaffRegistryPage(string? search = null);
    bool Create(CreateStaffViewModel model);
    EditStaffViewModel? GetStaffForEdit(int id);
    bool Update(EditStaffViewModel model);
    bool TryDelete(int id, out string? error);
}
