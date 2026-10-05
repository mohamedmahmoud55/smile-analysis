using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Helpers;

internal static class AppointmentCompletionHelper
{
    /// <summary>Marks scheduled appointments as completed once their end time has passed.</summary>
    public static void CompletePastScheduled(IUnitOfWork unitOfWork)
    {
        var now = DateTime.Now;
        var repo = unitOfWork.GetRepository<Appointment>();
        var past = repo.GetAll()
            .Where(a => a.Status == AppointmentStatus.Scheduled
                        && a.AppointmentTime.AddMinutes(a.DurationMinutes) <= now)
            .ToList();

        if (past.Count == 0)
            return;

        foreach (var a in past)
        {
            a.Status = AppointmentStatus.Completed;
            a.UpdatedAt = DateTime.UtcNow;
            repo.Update(a);
        }

        unitOfWork.SaveChanges();
    }
}
