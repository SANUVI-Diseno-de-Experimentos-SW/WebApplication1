using WebApplication1.Consultation.Domain.Models.Entities;
using WebApplication1.Consultation.Domain.Models.Enum;

namespace WebApplication1.Consultation.Infrastructure.Mappers;

public static class ConsultationMapper
{
    public static Domain.Models.Aggregate.Consultation ToDomain(dynamic document)
    {
        // ✅ Usar "ConsultationId" (con mayúscula) como viene del documento
        string id = document.ConsultationId ?? document.id;
        
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Document must have an id");
        }

        var messages = ((IEnumerable<dynamic>)document.Messages ?? Enumerable.Empty<dynamic>())
            .Select((dynamic message) =>
                new Message(
                    (string)message.Id,
                    (string)message.SenderId,
                    MessageSenderExtensions.FromString((string)message.SenderRole),
                    (string)message.Content,
                    (DateTime)message.SentAt
                )
            ).ToList();

        return new Domain.Models.Aggregate.Consultation(
            id,
            (string)document.PatientId,
            (string)document.MotherId,
            (string)document.NurseId,
            messages,
            (DateTime)document.CreatedAt,
            document.ClosedAt != null ? (DateTime?)document.ClosedAt : null
        );
    }
}