using MongoDB.Driver;
using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Infrastructure.Mappers;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;

public class MongoTreatmentRepository : ITreatmentRepository
{
    private readonly IMongoCollection<TreatmentDocument> _collection;
    private readonly ILogger<MongoTreatmentRepository> _logger;

    public MongoTreatmentRepository(IMongoDatabase database, ILogger<MongoTreatmentRepository> logger)
    {
        _collection = database.GetCollection<TreatmentDocument>("treatments");
        _logger = logger;
    }

    public async Task SaveAsync(Treatment treatment)
    {
        var treatmentData = treatment.ToPrimitives();
    
        var document = new TreatmentDocument
        {
            TreatmentId = treatmentData.Id,
            PatientId = treatmentData.PatientId,
            NurseId = treatmentData.NurseId,
            Supplement = treatmentData.Supplement,
            Quantity = treatmentData.Quantity,
            DosingHours = treatmentData.DosingHours,
            DurationDays = treatmentData.DurationDays,
            StartDate = treatmentData.StartDate,
            EndDate = treatmentData.EndDate,
            Status = treatmentData.Status,
            AdherenceScore = treatmentData.AdherenceScore,
            CurrentStreak = treatmentData.CurrentStreak,
            TotalConfirmed = treatmentData.TotalConfirmed,
            TotalOmitted = treatmentData.TotalOmitted,
            CompletionObservation = treatmentData.CompletionObservation,
            AbandonmentObservation = treatmentData.AbandonmentObservation,
            // ✅ CORREGIDO: Usar "RiskScore" (mayúscula) para la clase C#
            RiskScore = new RiskScoreDocument
            {
                Id = treatmentData.RiskScore?.Id ?? Guid.NewGuid().ToString(),
                Score = treatmentData.RiskScore?.Score ?? 0,
                RiskLevel = treatmentData.RiskScore?.RiskLevel ?? "LOW",
                CalculatedAt = treatmentData.RiskScore?.CalculatedAt ?? DateTime.UtcNow
            }
        };

        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Tratamiento guardado: {TreatmentId}", document.TreatmentId);
    }

    public async Task<Treatment?> FindActiveByPatientIdAsync(string patientId)
    {
        var filter = Builders<TreatmentDocument>.Filter.And(
            Builders<TreatmentDocument>.Filter.Eq(x => x.PatientId, patientId),
            Builders<TreatmentDocument>.Filter.Eq(x => x.Status, TreatmentStatus.ACTIVE.ToStringValue())
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return TreatmentMapper.ToDomain(document);
    }

    public async Task<List<Treatment>> FindAllActiveAsync()
    {
        var filter = Builders<TreatmentDocument>.Filter.Eq(x => x.Status, TreatmentStatus.ACTIVE.ToStringValue());
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(TreatmentMapper.ToDomain).ToList();
    }

    public async Task<Treatment?> FindByIdAsync(string treatmentId)
    {
        var filter = Builders<TreatmentDocument>.Filter.Eq(x => x.TreatmentId, treatmentId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return TreatmentMapper.ToDomain(document);
    }

    public async Task<List<Treatment>> FindByNurseIdAsync(string nurseId, TreatmentStatus? status = null)
    {
        var filterBuilder = Builders<TreatmentDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.NurseId, nurseId);

        if (status.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Status, status.Value.ToStringValue()));
        }

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(TreatmentMapper.ToDomain).ToList();
    }

    public async Task<List<Treatment>> FindByPatientIdAsync(string patientId)
    {
        var filter = Builders<TreatmentDocument>.Filter.Eq(x => x.PatientId, patientId);
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(TreatmentMapper.ToDomain).ToList();
    }

    public async Task<List<Treatment>> FindByRiskLevelAsync(RiskLevel riskLevel, string? nurseId = null)
    {
        var filterBuilder = Builders<TreatmentDocument>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Eq(x => x.Status, TreatmentStatus.ACTIVE.ToStringValue()),
            filterBuilder.Eq(x => x.RiskScore.RiskLevel, riskLevel.ToStringValue())
        );

        if (!string.IsNullOrEmpty(nurseId))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.NurseId, nurseId));
        }

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(TreatmentMapper.ToDomain).ToList();
    }

    public async Task UpdateAsync(Treatment treatment)
    {
        var treatmentData = treatment.ToPrimitives();
        var treatmentId = treatmentData.Id;

        var filter = Builders<TreatmentDocument>.Filter.Eq(x => x.TreatmentId, treatmentId);

        var update = Builders<TreatmentDocument>.Update
            .Set(x => x.Status, treatmentData.Status)
            .Set(x => x.AdherenceScore, treatmentData.AdherenceScore)
            .Set(x => x.CurrentStreak, treatmentData.CurrentStreak)
            .Set(x => x.TotalConfirmed, treatmentData.TotalConfirmed)
            .Set(x => x.TotalOmitted, treatmentData.TotalOmitted)
            .Set(x => x.CompletionObservation, treatmentData.CompletionObservation)
            .Set(x => x.AbandonmentObservation, treatmentData.AbandonmentObservation)
            .Set(x => x.RiskScore, new RiskScoreDocument
            {
                Id = treatmentData.RiskScore?.Id ?? Guid.NewGuid().ToString(),
                Score = treatmentData.RiskScore?.Score ?? 0,
                RiskLevel = treatmentData.RiskScore?.RiskLevel ?? "LOW",
                CalculatedAt = treatmentData.RiskScore?.CalculatedAt ?? DateTime.UtcNow
            });

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Tratamiento actualizado: {TreatmentId}", treatmentId);
    }

    public async Task DeleteAsync(string treatmentId)
    {
        var filter = Builders<TreatmentDocument>.Filter.Eq(x => x.TreatmentId, treatmentId);
        await _collection.DeleteOneAsync(filter);
        _logger.LogInformation("Tratamiento eliminado: {TreatmentId}", treatmentId);
    }
}