using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Model.Queries;

public record GetPatientsByRiskLevelQuery(
    RiskLevel RiskLevel,
    string? NurseId = null
);