namespace WebApplication1.Consultation.Domain.Models.Commands;

public record CloseConsultationCommand(
    string ConsultationId,
    string NurseId
);