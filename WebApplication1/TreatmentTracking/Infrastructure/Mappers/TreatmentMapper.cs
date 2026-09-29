using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.TreatmentTracking.Infrastructure.Mappers;

public static class TreatmentMapper
{
    public static Treatment ToDomain(TreatmentDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // Crear RiskScore desde el documento
        var riskScore = new RiskScore(
            document.RiskScore?.Id ?? Guid.NewGuid().ToString(),
            document.RiskScore?.Score ?? 10,
            RiskLevelExtensions.FromString(document.RiskScore?.RiskLevel ?? "LOW"),
            document.RiskScore?.CalculatedAt ?? DateTime.UtcNow
        );

        return new Treatment(
            document.TreatmentId,
            document.PatientId,
            document.NurseId,
            document.Supplement,
            document.Quantity,
            document.DosingHours,
            document.DurationDays,
            document.StartDate,
            document.EndDate,
            TreatmentStatusExtensions.FromString(document.Status),
            document.AdherenceScore,
            document.CurrentStreak,
            document.TotalConfirmed,
            document.TotalOmitted,
            document.CompletionObservation,
            document.AbandonmentObservation,
            riskScore
        );
    }

    public static TreatmentDocument ToPersistence(Treatment treatment)
    {
        if (treatment == null)
            throw new ArgumentNullException(nameof(treatment));

        var data = treatment.ToPrimitives();

        return new TreatmentDocument
        {
            TreatmentId = data.Id,
            PatientId = data.PatientId,
            NurseId = data.NurseId,
            Supplement = data.Supplement,
            Quantity = data.Quantity,
            DosingHours = data.DosingHours,
            DurationDays = data.DurationDays,
            StartDate = data.StartDate,
            EndDate = data.EndDate,
            Status = data.Status,
            AdherenceScore = data.AdherenceScore,
            CurrentStreak = data.CurrentStreak,
            TotalConfirmed = data.TotalConfirmed,
            TotalOmitted = data.TotalOmitted,
            CompletionObservation = data.CompletionObservation,
            AbandonmentObservation = data.AbandonmentObservation,
            RiskScore = new RiskScoreDocument
            {
                Id = data.RiskScore?.Id ?? Guid.NewGuid().ToString(),
                Score = data.RiskScore?.Score ?? 10,
                RiskLevel = data.RiskScore?.RiskLevel ?? "LOW",
                CalculatedAt = data.RiskScore?.CalculatedAt ?? DateTime.UtcNow
            }
        };
    }
}