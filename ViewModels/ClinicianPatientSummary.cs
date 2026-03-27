namespace Sensore.ViewModels;

public class ClinicianPatientSummary
{
    public int PatientId { get; init; }

    public string PatientName { get; init; } = string.Empty;

    public double LatestPpi { get; init; }

    public int ActiveAlertCount { get; init; }

    public DateTime? LastUpdated { get; init; }
}
