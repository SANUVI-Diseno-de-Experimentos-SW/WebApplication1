using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class GetOpenConsultationsByNurseQueryAssembler
{
    public static GetOpenConsultationsByNurseQuery ToQuery(string nurseId, string? searchTerm = null)
    {
        return new GetOpenConsultationsByNurseQuery(nurseId, searchTerm);
    }
}