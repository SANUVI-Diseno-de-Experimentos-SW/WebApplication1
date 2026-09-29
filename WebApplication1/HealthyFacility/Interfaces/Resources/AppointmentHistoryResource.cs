namespace WebApplication1.HealthyFacility.Interfaces.Resources;

public class AppointmentHistoryResource
{
    public string AppointmentId { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string AppointmentDate { get; set; } = string.Empty;
    public string AppointmentTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}