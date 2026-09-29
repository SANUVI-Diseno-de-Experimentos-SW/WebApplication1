namespace WebApplication1.Consultation.Interfaces.Resources;

public class GetMessagesAfterResource
{
    public string ConsultationId { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
    public long AfterTimestamp { get; set; }
    public int? Limit { get; set; }
}