namespace WebApplication1.patient_management.Interfaces.Resources;

public class PatientResource
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusAssignment { get; set; } = string.Empty;
}