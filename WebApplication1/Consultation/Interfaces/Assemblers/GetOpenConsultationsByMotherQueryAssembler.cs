using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class GetOpenConsultationsByMotherQueryAssembler
{
    public static GetOpenConsultationsByMotherQuery ToQuery(string motherId)
    {
        return new GetOpenConsultationsByMotherQuery(motherId);
    }
}