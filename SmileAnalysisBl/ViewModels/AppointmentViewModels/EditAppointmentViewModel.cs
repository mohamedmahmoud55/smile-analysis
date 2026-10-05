using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmileAnalysisBl.ViewModels.AppointmentViewModels;

public class EditAppointmentViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a patient.")]
    [Display(Name = "Patient")]
    public int? PatientId { get; set; }

    [Required(ErrorMessage = "Please select a doctor.")]
    [Display(Name = "Doctor")]
    public int? DoctorId { get; set; }

    [Required(ErrorMessage = "Please choose an appointment date.")]
    [Display(Name = "Date")]
    public DateOnly? AppointmentDate { get; set; }

    [Required(ErrorMessage = "Please choose a start time.")]
    [Display(Name = "Start time")]
    public TimeOnly? AppointmentStartTime { get; set; }

    [Range(5, 480)]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; } = 30;

    [BindNever]
    public List<SelectListItem> Patients { get; set; } = new();

    [BindNever]
    public List<SelectListItem> Doctors { get; set; } = new();
}
