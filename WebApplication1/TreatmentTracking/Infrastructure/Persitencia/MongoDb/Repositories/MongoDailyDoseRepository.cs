using MongoDB.Driver;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Infrastructure.Mappers;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;

public class MongoDailyDoseRepository : IDailyDoseRepository
{
    private readonly IMongoCollection<DailyDoseDocument> _collection;
    private readonly ILogger<MongoDailyDoseRepository> _logger;

    public MongoDailyDoseRepository(IMongoDatabase database, ILogger<MongoDailyDoseRepository> logger)
    {
        _collection = database.GetCollection<DailyDoseDocument>("dailydoses");
        _logger = logger;
    }

    public async Task<DailyDose?> FindByIdAsync(string dailyDoseId)
    {
        var filter = Builders<DailyDoseDocument>.Filter.Eq(x => x.DailyDoseId, dailyDoseId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document == null ? null : DailyDoseMapper.ToDomain(document);
    }

    public async Task<List<DailyDose>> FindByTreatmentIdAsync(string treatmentId)
    {
        var filter = Builders<DailyDoseDocument>.Filter.Eq(x => x.TreatmentId, treatmentId);
        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Select(DailyDoseMapper.ToDomain).ToList();
    }

    public async Task<List<DailyDose>> FindPendingOlderThanHoursAsync(int hours)
    {
        var threshold = DateTime.UtcNow.AddHours(-hours);

        var filter = Builders<DailyDoseDocument>.Filter.And(
            Builders<DailyDoseDocument>.Filter.Eq(x => x.Status, DoseStatus.PENDING.ToStringValue()),
            Builders<DailyDoseDocument>.Filter.Lte(x => x.ScheduledDate, threshold)
        );

        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Select(DailyDoseMapper.ToDomain).ToList();
    }

    public async Task<DailyDose?> FindTodayDoseAsync(string treatmentId)
    {
        var today = DateTime.UtcNow.Date;
        var start = today;
        var end = today.AddDays(1).AddMilliseconds(-1);

        var filter = Builders<DailyDoseDocument>.Filter.And(
            Builders<DailyDoseDocument>.Filter.Eq(x => x.TreatmentId, treatmentId),
            Builders<DailyDoseDocument>.Filter.Gte(x => x.ScheduledDate, start),
            Builders<DailyDoseDocument>.Filter.Lte(x => x.ScheduledDate, end)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document == null ? null : DailyDoseMapper.ToDomain(document);
    }

    public async Task SaveAsync(DailyDose dose)
    {
        var document = DailyDoseMapper.ToPersistence(dose);
        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Dosis diaria guardada: {DailyDoseId}", document.DailyDoseId);
    }

    public async Task SaveManyAsync(List<DailyDose> doses)
    {
        var documents = doses.Select(DailyDoseMapper.ToPersistence).ToList();
        await _collection.InsertManyAsync(documents);
        _logger.LogInformation("{Count} dosis diarias guardadas", documents.Count);
    }

    public async Task UpdateAsync(DailyDose dose)
    {
        var document = DailyDoseMapper.ToPersistence(dose);
        var filter = Builders<DailyDoseDocument>.Filter.Eq(x => x.DailyDoseId, document.DailyDoseId);

        var update = Builders<DailyDoseDocument>.Update
            .Set(x => x.Status, document.Status)
            .Set(x => x.ConfirmedAt, document.ConfirmedAt);

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Dosis diaria actualizada: {DailyDoseId}", document.DailyDoseId);
    }

    public async Task DeleteAsync(string dailyDoseId)
    {
        var filter = Builders<DailyDoseDocument>.Filter.Eq(x => x.DailyDoseId, dailyDoseId);
        await _collection.DeleteOneAsync(filter);
        _logger.LogInformation("Dosis diaria eliminada: {DailyDoseId}", dailyDoseId);
    }

    public async Task DeleteManyAsync(List<string> dailyDoseIds)
    {
        var filter = Builders<DailyDoseDocument>.Filter.In(x => x.DailyDoseId, dailyDoseIds);
        await _collection.DeleteManyAsync(filter);
        _logger.LogInformation("{Count} dosis diarias eliminadas", dailyDoseIds.Count);
    }
}