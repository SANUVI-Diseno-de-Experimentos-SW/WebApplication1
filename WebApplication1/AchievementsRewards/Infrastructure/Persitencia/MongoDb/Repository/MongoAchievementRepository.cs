using MongoDB.Driver;
using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Infrastructure.Mappers;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;

public class MongoAchievementRepository : IAchievementRepository
{
    private readonly IMongoCollection<AchievementDocument> _collection;
    private readonly ILogger<MongoAchievementRepository> _logger;

    public MongoAchievementRepository(IMongoDatabase database, ILogger<MongoAchievementRepository> logger)
    {
        _collection = database.GetCollection<AchievementDocument>("achievements");
        _logger = logger;
    }

    public async Task SaveAsync(Achievement achievement)
    {
        try
        {
            var document = AchievementMapper.ToPersistence(achievement);
            await _collection.InsertOneAsync(document);
            _logger.LogInformation("✅ Achievement guardado: {AchievementId}", document.AchievementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando Achievement");
            throw;
        }
    }

    public async Task UpdateAsync(Achievement achievement)
    {
        try
        {
            var document = AchievementMapper.ToPersistence(achievement);
            
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.AchievementId, document.AchievementId);
            
            var update = Builders<AchievementDocument>.Update
                .Set(x => x.PatientId, document.PatientId)
                .Set(x => x.MotherId, document.MotherId)
                .Set(x => x.TreatmentId, document.TreatmentId)
                .Set(x => x.DurationDays, document.DurationDays)
                .Set(x => x.CurrentStreak, document.CurrentStreak)
                .Set(x => x.LongestStreak, document.LongestStreak)
                .Set(x => x.BestStreak, document.BestStreak)
                .Set(x => x.StreakStartDate, document.StreakStartDate)
                .Set(x => x.TotalPoints, document.TotalPoints)
                .Set(x => x.Status, document.Status)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update);
            _logger.LogInformation("✅ Achievement actualizado: {AchievementId}", document.AchievementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando Achievement");
            throw;
        }
    }

    public async Task<Achievement?> FindByIdAsync(string id)
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.AchievementId, id);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            return AchievementMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Achievement por ID: {Id}", id);
            throw;
        }
    }

    public async Task<Achievement?> FindByTreatmentIdAsync(string treatmentId)
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.TreatmentId, treatmentId);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            return AchievementMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Achievement por TreatmentId: {TreatmentId}", treatmentId);
            throw;
        }
    }

    public async Task<Achievement?> FindByPatientIdAsync(string patientId)
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.PatientId, patientId);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            return AchievementMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Achievement por PatientId: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<List<Achievement>> FindByMotherIdAsync(string motherId)
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.MotherId, motherId);
            var documents = await _collection.Find(filter).ToListAsync();

            return documents.Select(AchievementMapper.ToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Achievements por MotherId: {MotherId}", motherId);
            throw;
        }
    }

    public async Task<List<Achievement>> FindAllActiveAsync()
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.Status, AchievementStatus.ACTIVE.ToStringValue());
            var documents = await _collection.Find(filter).ToListAsync();

            return documents.Select(AchievementMapper.ToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Achievements activos");
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<AchievementDocument>.Filter.Eq(x => x.AchievementId, id);
            await _collection.DeleteOneAsync(filter);
            _logger.LogInformation("✅ Achievement eliminado: {AchievementId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando Achievement: {Id}", id);
            throw;
        }
    }
}