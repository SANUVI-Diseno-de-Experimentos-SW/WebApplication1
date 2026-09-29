namespace WebApplication1.TreatmentTracking.Interfaces.Resources;

public class StartTreatmentResource
{
    public string PatientId { get; set; } = string.Empty;
    public string NurseId { get; set; } = string.Empty;
    public string SupplementName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string DosingHours { get; set; } = string.Empty;
    public int DurationDays { get; set; }
}