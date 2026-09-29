using WebApplication1.Consultation.Domain.Models.Commands;
using WebApplication1.Consultation.Interfaces.Resources;

namespace WebApplication1.Consultation.Interfaces.Assemblers;

public static class StartConsultationCommandFromResourceAssembler
{
    public static StartConsultationCommand ToCommand(StartConsultationResource resource)
    {
        return new StartConsultationCommand(
            resource.MotherId,
            resource.PatientId,
            resource.FirstMessageContent
        );
    }
}