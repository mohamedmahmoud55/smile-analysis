namespace SmileAnalysisDal.Entities;

public class GummySmileCase : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string GsCaseId { get; set; } = null!;
    public string GsStatus { get; set; } = "created";
    public string? DiagnosisSummary { get; set; }
    public string? Severity { get; set; }
    public string? ReportJson { get; set; }
    public byte[]? ReportPdf { get; set; }
    public string? ClinicalDraftJson { get; set; }
    public string? ClinicalRequestJson { get; set; }
    public string? OverridesRequestJson { get; set; }
    public string? CaseSnapshotJson { get; set; }
    public byte[]? RestImage { get; set; }
    public string? RestImageContentType { get; set; }
    public byte[]? SmileImage { get; set; }
    public string? SmileImageContentType { get; set; }
    public string? CachedAssetsJson { get; set; }
    public DateTime? CompletedAt { get; set; }
}
