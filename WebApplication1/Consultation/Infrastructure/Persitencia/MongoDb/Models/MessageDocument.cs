namespace WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Models;

public class MessageDocument
{
    public string Id { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}