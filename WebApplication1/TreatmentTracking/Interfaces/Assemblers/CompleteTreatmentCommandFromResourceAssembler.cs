using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces.Assemblers;


public static class CompleteTreatmentCommandFromResourceAssembler
{
    public static CompleteTreatmentCommand ToCommand(CompleteTreatmentResource resource)
    {
        if (string.IsNullOrEmpty(resource.NurseId))
        {
            throw new Exception("nurseId es requerido");
        }

        return new CompleteTreatmentCommand(
            resource.TreatmentId,
            resource.NurseId,
            resource.Observation
        );
    }
}