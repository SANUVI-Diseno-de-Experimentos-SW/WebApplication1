using WebApplication1.Consultation.Domain.Models.Enum;

namespace WebApplication1.Consultation.Domain.Models.Commands;

public record AddMessageCommand(
    string ConsultationId,
    string SenderId,
    MessageSender SenderRole,
    string Content
);