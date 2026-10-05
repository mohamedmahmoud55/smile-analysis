using SmileAnalysisBl.Helpers;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisBl.ViewModels.AnalyticsViewModels;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Entities.Enums;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisBl.Services.Classes;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public AnalyticsViewModel GetAnalyticsData()
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var appointments = _unitOfWork.GetRepository<Appointment>().GetAll();
        return new AnalyticsViewModel
        {
            TotalPatients = _unitOfWork.GetRepository<Patient>().GetAll().Count(),
            TotalDoctors = _unitOfWork.GetRepository<Doctor>().GetAll().Count(),
            TotalStaff = _unitOfWork.GetRepository<Staff>().GetAll().Count(),
            ScheduledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
        };
    }

    public OrthoDashboardViewModel GetOrthoDashboard()
    {
        AppointmentCompletionHelper.CompletePastScheduled(_unitOfWork);

        var today = DateTime.Today;
        var appointments = _unitOfWork.GetRepository<Appointment>().GetAll().ToList();
        var patients = _unitOfWork.GetRepository<Patient>().GetAll().ToList();

        var startOfWeek = StartOfWeekMonday(today);
        var lastWeekStart = startOfWeek.AddDays(-7);

        var todayAppts = appointments.Count(a => a.AppointmentTime.Date == today);
        var newConsultToday = patients.Count(p => p.CreatedAt.Date == today);
        var upcomingAppointments = appointments.Count(a =>
            a.Status == AppointmentStatus.Scheduled &&
            a.AppointmentTime.Date >= today);

        var completedThisWeek = appointments.Count(a =>
            a.Status == AppointmentStatus.Completed
            && a.AppointmentTime.Date >= startOfWeek
            && a.AppointmentTime.Date <= today);

        var completedLastWeek = appointments.Count(a =>
            a.Status == AppointmentStatus.Completed
            && a.AppointmentTime.Date >= lastWeekStart
            && a.AppointmentTime.Date < startOfWeek);

        var lastApptByPatient = appointments
            .GroupBy(a => a.PatientId)
            .ToDictionary(g => g.Key, g => g.Max(a => a.AppointmentTime));

        var upcomingByPatient = appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentTime.Date >= today)
            .GroupBy(a => a.PatientId)
            .ToDictionary(g => g.Key, g => g.Min(a => a.AppointmentTime));

        var recentRows = patients
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .Select(p => MapRecentPatient(p, lastApptByPatient, upcomingByPatient, today))
            .ToList();

        return new OrthoDashboardViewModel
        {
            TodayAppointments = todayAppts,
            NewConsultationsToday = newConsultToday,
            UpcomingAppointments = upcomingAppointments,
            CompletedThisWeek = completedThisWeek,
            CompletedDeltaVsLastWeek = completedThisWeek - completedLastWeek,
            RecentPatients = recentRows
        };
    }

    private static DateTime StartOfWeekMonday(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.AddDays(-diff).Date;
    }

    private static RecentPatientRowViewModel MapRecentPatient(
        Patient p,
        Dictionary<int, DateTime> lastApptByPatient,
        Dictionary<int, DateTime> upcomingByPatient,
        DateTime today)
    {
        var age = (int)Math.Floor((today - p.DateOfBirth.Date).TotalDays / 365.25);
        if (age < 0) age = 0;

        lastApptByPatient.TryGetValue(p.Id, out var lastAppt);
        var lastVisitDisplay = lastAppt == default
            ? "—"
            : lastAppt.ToString("MMM d, yyyy");

        string variant;
        string label;

        if (upcomingByPatient.ContainsKey(p.Id))
        {
            variant = "primary";
            label = "Scheduled";
        }
        else if (lastAppt != default)
        {
            variant = "ortho-purple";
            label = "Returning";
        }
        else
        {
            variant = "success";
            label = "New Patient";
        }

        return new RecentPatientRowViewModel
        {
            Id = p.Id,
            FullName = $"{p.FirstName} {p.LastName}".Trim(),
            Age = age,
            LastVisitDisplay = lastVisitDisplay,
            StatusBadgeVariant = variant,
            StatusLabel = label
        };
    }
}
