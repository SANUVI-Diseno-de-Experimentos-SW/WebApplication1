namespace WebApplication1.Consultation.Domain.Models.Queries;

public record GetConsultationChatQuery(
    string ConsultationId,
    string RequesterId
);