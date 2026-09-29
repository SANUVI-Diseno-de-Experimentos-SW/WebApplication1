using WebApplication1.AchievementsRewards.Domain.Model.Entities;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Infrastructure.Mappers;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class MongoBadgeRepository : IBadgeRepository
{
    private readonly IMongoCollection<BadgeDocument> _collection;
    private readonly ILogger<MongoBadgeRepository> _logger;

    public MongoBadgeRepository(IMongoDatabase database, ILogger<MongoBadgeRepository> logger)
    {
        _collection = database.GetCollection<BadgeDocument>("badges");
        _logger = logger;
    }

    public async Task SaveAsync(Badge badge)
    {
        try
        {
            var document = BadgeMapper.ToPersistence(badge);
            await _collection.InsertOneAsync(document);
            _logger.LogInformation("✅ Badge guardado: {BadgeId}", document.BadgeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando Badge");
            throw;
        }
    }

    public async Task SaveManyAsync(List<Badge> badges)
    {
        try
        {
            var documents = badges.Select(BadgeMapper.ToPersistence).ToList();
            await _collection.InsertManyAsync(documents);
            _logger.LogInformation("✅ {Count} badges guardados", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando múltiples badges");
            throw;
        }
    }

    public async Task UpdateAsync(Badge badge)
    {
        try
        {
            var document = BadgeMapper.ToPersistence(badge);
            
            var filter = Builders<BadgeDocument>.Filter.Eq(x => x.BadgeId, document.BadgeId);
            
            var update = Builders<BadgeDocument>.Update
                .Set(x => x.AchievementId, document.AchievementId)
                .Set(x => x.Type, document.Type)
                .Set(x => x.Name, document.Name)
                .Set(x => x.Description, document.Description)
                .Set(x => x.Milestone, document.Milestone)
                .Set(x => x.IsUnlocked, document.IsUnlocked)
                .Set(x => x.UnlockedAt, document.UnlockedAt)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update);
            _logger.LogInformation("✅ Badge actualizado: {BadgeId}", document.BadgeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando Badge");
            throw;
        }
    }

    public async Task UpdateManyAsync(List<Badge> badges)
    {
        try
        {
            var bulkOps = badges.Select(badge =>
            {
                var document = BadgeMapper.ToPersistence(badge);
                
                var update = Builders<BadgeDocument>.Update
                    .Set(x => x.IsUnlocked, document.IsUnlocked)
                    .Set(x => x.UnlockedAt, document.UnlockedAt)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);

                return new UpdateOneModel<BadgeDocument>(
                    Builders<BadgeDocument>.Filter.Eq(x => x.BadgeId, document.BadgeId),
                    update
                );
            }).ToList();

            await _collection.BulkWriteAsync(bulkOps);
            _logger.LogInformation("✅ {Count} badges actualizados", badges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando múltiples badges");
            throw;
        }
    }

    public async Task<Badge?> FindByIdAsync(string id)
    {
        try
        {
            var filter = Builders<BadgeDocument>.Filter.Eq(x => x.BadgeId, id);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            return BadgeMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Badge por ID: {Id}", id);
            throw;
        }
    }

    public async Task<List<Badge>> FindByAchievementIdAsync(string achievementId)
    {
        try
        {
            var filter = Builders<BadgeDocument>.Filter.Eq(x => x.AchievementId, achievementId);
            var documents = await _collection.Find(filter).ToListAsync();

            return documents.Select(BadgeMapper.ToDomain).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Badges por AchievementId: {AchievementId}", achievementId);
            throw;
        }
    }

    public async Task<Badge?> FindByAchievementIdAndTypeAsync(string achievementId, BadgeType type)
    {
        try
        {
            var filter = Builders<BadgeDocument>.Filter.And(
                Builders<BadgeDocument>.Filter.Eq(x => x.AchievementId, achievementId),
                Builders<BadgeDocument>.Filter.Eq(x => x.Type, type.ToStringValue())
            );

            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            return BadgeMapper.ToDomain(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error buscando Badge por AchievementId y Type");
            throw;
        }
    }

    public async Task DeleteByAchievementIdAsync(string achievementId)
    {
        try
        {
            var filter = Builders<BadgeDocument>.Filter.Eq(x => x.AchievementId, achievementId);
            await _collection.DeleteManyAsync(filter);
            _logger.LogInformation("✅ Badges eliminados para achievement: {AchievementId}", achievementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando badges por AchievementId: {AchievementId}", achievementId);
            throw;
        }
    }
}