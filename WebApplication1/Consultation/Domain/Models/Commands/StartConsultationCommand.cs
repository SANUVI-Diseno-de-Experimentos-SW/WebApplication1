namespace WebApplication1.Consultation.Domain.Models.Commands;

public record StartConsultationCommand(
    string MotherId,
    string PatientId,
    string FirstMessageContent
);