using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces.Assemblers;

public static class AbandonTreatmentCommandFromResourceAssembler
{
    public static AbandonTreatmentCommand ToCommand(AbandonTreatmentResource resource)
    {
        if (string.IsNullOrEmpty(resource.NurseId))
        {
            throw new Exception("nurseId es requerido");
        }

        return new AbandonTreatmentCommand(
            resource.TreatmentId,
            resource.NurseId,
            resource.Observation
        );
    }
}