using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces.Assemblers;


public static class ConfirmDoseCommandFromResourceAssembler
{
    public static ConfirmDoseCommand ToCommand(ConfirmDoseResource resource, string motherId)
    {
        return new ConfirmDoseCommand(
            resource.PatientId,
            motherId
        );
    }
}