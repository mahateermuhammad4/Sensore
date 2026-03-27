namespace Sensore.Models;

public class Report
{
    public int ReportId { get; set; }

    public int PatientId { get; set; }

    public int GeneratedBy { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    public string ReportContent { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; }
}
