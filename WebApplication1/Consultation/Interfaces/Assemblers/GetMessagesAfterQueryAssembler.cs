using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;



public static class GetMessagesAfterQueryAssembler
{
    public static GetMessagesAfterQuery ToQuery(
        string consultationId,
        string requesterId,
        long afterTimestamp,
        int? limit = null)
    {
        return new GetMessagesAfterQuery(consultationId, requesterId, afterTimestamp, limit);
    }
}