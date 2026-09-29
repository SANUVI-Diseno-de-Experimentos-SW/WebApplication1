using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces.Assemblers;

public static class StartTreatmentCommandFromResourceAssembler
{
    public static StartTreatmentCommand ToCommand(StartTreatmentResource resource)
    {
        return new StartTreatmentCommand(
            resource.PatientId,
            resource.NurseId,
            resource.SupplementName,
            resource.Quantity,
            resource.DosingHours,
            resource.DurationDays
        );
    }
}