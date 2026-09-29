namespace WebApplication1.TreatmentTracking.Domain.Model.Queries;

public record GetTodayDoseQuery(
    string PatientId,
    string MotherId
);