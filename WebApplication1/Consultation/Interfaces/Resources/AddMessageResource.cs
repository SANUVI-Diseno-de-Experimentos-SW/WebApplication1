namespace WebApplication1.Consultation.Interfaces.Resources;

public class AddMessageResource
{
    public string ConsultationId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}