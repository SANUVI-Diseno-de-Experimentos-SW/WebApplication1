using WebApplication1.Consultation.Domain.Models.Commands;
using WebApplication1.Consultation.Domain.Models.Enum;
using WebApplication1.Consultation.Interfaces.Resources;

namespace WebApplication1.Consultation.Interfaces.Assemblers;




public static class AddMessageCommandFromResourceAssembler
{
    public static AddMessageCommand ToCommand(AddMessageResource resource)
    {
        return new AddMessageCommand(
            resource.ConsultationId,
            resource.SenderId,
            MessageSenderExtensions.FromString(resource.SenderRole),
            resource.Content
        );
    }
}