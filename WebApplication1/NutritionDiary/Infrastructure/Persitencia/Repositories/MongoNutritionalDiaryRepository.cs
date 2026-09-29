using MongoDB.Driver;
using WebApplication1.NutritionDiary.Domain.Models.Aggregate;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Infrastructure.Mappers;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Repositories;

public class MongoNutritionalDiaryRepository : INutritionalDiaryRepository
{
    private readonly IMongoCollection<NutritionalDiaryDocument> _collection;
    private readonly ILogger<MongoNutritionalDiaryRepository> _logger;

    public MongoNutritionalDiaryRepository(IMongoDatabase database, ILogger<MongoNutritionalDiaryRepository> logger)
    {
        _collection = database.GetCollection<NutritionalDiaryDocument>("nutritionaldiaries");
        _logger = logger;
    }

    public async Task SaveAsync(NutritionalDiary diary)
    {
        var document = NutritionalDiaryMapper.ToPersistence(diary);
        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Diario nutricional guardado: {NutritionalDiaryId}", document.NutritionalDiaryId);
    }

    public async Task UpdateAsync(NutritionalDiary diary)
    {
        var document = NutritionalDiaryMapper.ToPersistence(diary);
        var filter = Builders<NutritionalDiaryDocument>.Filter.Eq(x => x.NutritionalDiaryId, document.NutritionalDiaryId);

        var update = Builders<NutritionalDiaryDocument>.Update
            .Set(x => x.TotalIronAbsorbed, document.TotalIronAbsorbed)
            .Set(x => x.HasInhibitor, document.HasInhibitor);

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Diario nutricional actualizado: {NutritionalDiaryId}", document.NutritionalDiaryId);
    }

    public async Task<NutritionalDiary?> FindTodayByPatientIdAsync(string patientId)
    {
        var now = DateTime.UtcNow;
        var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999, DateTimeKind.Utc);

        var filter = Builders<NutritionalDiaryDocument>.Filter.And(
            Builders<NutritionalDiaryDocument>.Filter.Eq(x => x.PatientId, patientId),
            Builders<NutritionalDiaryDocument>.Filter.Gte(x => x.Date, startOfDay),
            Builders<NutritionalDiaryDocument>.Filter.Lte(x => x.Date, endOfDay)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return NutritionalDiaryMapper.ToDomain(document);
    }

    public async Task<List<NutritionalDiary>> FindByPatientAndDateRangeAsync(string patientId, DateTime startDate, DateTime endDate)
    {
        var startUtc = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, 999, DateTimeKind.Utc);

        var filter = Builders<NutritionalDiaryDocument>.Filter.And(
            Builders<NutritionalDiaryDocument>.Filter.Eq(x => x.PatientId, patientId),
            Builders<NutritionalDiaryDocument>.Filter.Gte(x => x.Date, startUtc),
            Builders<NutritionalDiaryDocument>.Filter.Lte(x => x.Date, endUtc)
        );

        var documents = await _collection.Find(filter).ToListAsync();

        return NutritionalDiaryMapper.ToDomainList(documents);
    }
}