using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces.Assemblers;

public static class EvaluateMissedDoseCommandFromResourceAssembler
{
    public static EvaluateMissedDoseCommand ToCommand(EvaluateMissedDoseResource resource)
    {
        return new EvaluateMissedDoseCommand(
            resource.DailyDoseId
        );
    }
}