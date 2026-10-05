namespace SmileAnalysisDal.Entities;

public class XRayAnalysisCase : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string? Summary { get; set; }
    public string? CvmStage { get; set; }
    public string? SkeletalClass { get; set; }
    public int? LandmarkCount { get; set; }
    public int ConfidencePercent { get; set; }

    public byte[]? PanoramicImage { get; set; }
    public string? PanoramicImageContentType { get; set; }
    public byte[]? CephImage { get; set; }
    public string? CephImageContentType { get; set; }
    public string? ResultsJson { get; set; }

    public DateTime? CompletedAt { get; set; }
}
