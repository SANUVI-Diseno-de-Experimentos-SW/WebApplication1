namespace WebApplication1.TreatmentTracking.Domain.Model.Queries;

public record GetPendingPatientsByNurseQuery(
    string NurseId
);