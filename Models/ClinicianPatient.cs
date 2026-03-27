namespace Sensore.Models;

public class ClinicianPatient
{
    public int ClinicianId { get; set; }

    public int PatientId { get; set; }

    public User? Clinician { get; set; }

    public User? Patient { get; set; }
}
