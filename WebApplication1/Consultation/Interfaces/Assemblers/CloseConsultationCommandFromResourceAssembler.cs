using WebApplication1.Consultation.Domain.Models.Commands;
using WebApplication1.Consultation.Interfaces.Resources;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class CloseConsultationCommandFromResourceAssembler
{
    public static CloseConsultationCommand ToCommand(CloseConsultationResource resource)
    {
        return new CloseConsultationCommand(
            resource.ConsultationId,
            resource.NurseId
        );
    }
}