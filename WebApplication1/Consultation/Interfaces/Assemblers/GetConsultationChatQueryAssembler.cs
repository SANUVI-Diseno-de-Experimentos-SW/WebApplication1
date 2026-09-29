using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class GetConsultationChatQueryAssembler
{
    public static GetConsultationChatQuery ToQuery(string consultationId, string requesterId)
    {
        return new GetConsultationChatQuery(consultationId, requesterId);
    }
}