namespace SmileAnalysisBl.Configuration;

public class ClinicOptions
{
    public const string SectionName = "Clinic";

    public string Name { get; set; } = "Smile Analysis Dental Clinic";
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
