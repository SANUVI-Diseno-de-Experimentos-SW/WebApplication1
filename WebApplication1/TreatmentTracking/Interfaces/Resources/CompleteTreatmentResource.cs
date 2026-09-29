namespace WebApplication1.TreatmentTracking.Interfaces.Resources;

public class CompleteTreatmentResource
{
    public string TreatmentId { get; set; } = string.Empty;
    public string? NurseId { get; set; }
    public string? Observation { get; set; }
}