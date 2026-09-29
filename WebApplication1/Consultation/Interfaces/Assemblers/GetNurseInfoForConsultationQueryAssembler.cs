using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class GetNurseInfoForConsultationQueryAssembler
{
    public static GetNurseInfoForConsultationQuery ToQuery(string patientId)
    {
        return new GetNurseInfoForConsultationQuery(patientId);
    }
}