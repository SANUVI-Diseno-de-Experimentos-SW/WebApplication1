namespace WebApplication1.Consultation.Interfaces.Resources;

public class StartConsultationResource
{
    public string MotherId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string FirstMessageContent { get; set; } = string.Empty;
}