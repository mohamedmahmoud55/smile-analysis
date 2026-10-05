using SmileAnalysisBl.ViewModels.AnalyticsViewModels;

namespace SmileAnalysisBl.Services.Interfaces
{
    public interface IAnalyticsService
    {
        AnalyticsViewModel GetAnalyticsData();
        OrthoDashboardViewModel GetOrthoDashboard();
    }
}
