using WebApplication1.TreatmentTracking.Domain.Model.Commands;

namespace WebApplication1.TreatmentTracking.Domain.Services;

public interface ITreatmentCommandService
{
    Task<object> StartTreatmentAsync(StartTreatmentCommand command);
    Task<object> ConfirmDoseAsync(ConfirmDoseCommand command);
    Task<object> CompleteTreatmentAsync(CompleteTreatmentCommand command);
    Task<object> AbandonTreatmentAsync(AbandonTreatmentCommand command);
    Task<object> EvaluateMissedDoseAsync(EvaluateMissedDoseCommand command);
    
    // Métodos solo para pruebas
    Task<object> ForceOmitDoseForTestingAsync(string dailyDoseId);
    Task<object> ForceConfirmDoseForTestingAsync(string dailyDoseId);
}
