using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class GetPatientsWithNurseAssignmentQueryAssembler
{
    public static GetPatientsWithNurseAssignmentQuery ToQuery(string motherId)
    {
        return new GetPatientsWithNurseAssignmentQuery(motherId);
    }
}