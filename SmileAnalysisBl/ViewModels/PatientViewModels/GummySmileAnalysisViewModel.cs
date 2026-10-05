using System.ComponentModel.DataAnnotations;

namespace SmileAnalysisBl.ViewModels.PatientViewModels;

public class GummySmileAnalysisViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;

    [Display(Name = "Upper lip length (mm)")]
    public decimal? UpperLipLength { get; set; }

    [Display(Name = "Gingival display (mm)")]
    public decimal? GingivalDisplay { get; set; }

    [Display(Name = "Smile line angle (°)")]
    public decimal? SmileLineAngle { get; set; }

    [Display(Name = "Interlabial gap (mm)")]
    public decimal? InterlabialGap { get; set; }

    [Display(Name = "Maxillary incisor display (mm)")]
    public decimal? MaxillaryIncisorDisplay { get; set; }

    [Display(Name = "Gummy smile ratio (%)")]
    public decimal? GummySmileRatio { get; set; }

    [Display(Name = "Lip mobility (mm)")]
    public decimal? LipMobility { get; set; }

    [Display(Name = "Vertical maxillary excess (mm)")]
    public decimal? VerticalMaxillaryExcess { get; set; }
}
