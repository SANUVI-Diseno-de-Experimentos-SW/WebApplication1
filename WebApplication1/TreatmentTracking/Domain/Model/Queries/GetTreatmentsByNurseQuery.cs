using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Model.Queries;

public record GetTreatmentsByNurseQuery(
    string NurseId,
    TreatmentStatus? Status = null
);