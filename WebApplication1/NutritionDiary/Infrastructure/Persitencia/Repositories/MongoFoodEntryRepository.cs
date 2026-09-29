using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Infrastructure.Mappers;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Repositories;

public class MongoFoodEntryRepository : IFoodEntryRepository
{
    private readonly IMongoCollection<FoodEntryDocument> _collection;
    private readonly ILogger<MongoFoodEntryRepository> _logger;

    public MongoFoodEntryRepository(IMongoDatabase database, ILogger<MongoFoodEntryRepository> logger)
    {
        _collection = database.GetCollection<FoodEntryDocument>("foodentries");
        _logger = logger;
    }

    public async Task SaveAsync(FoodEntry entry)
    {
        var document = FoodEntryMapper.ToPersistence(entry);
        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Food entry guardado: {FoodEntryId}", document.FoodEntryId);
    }

    public async Task<List<FoodEntry>> FindByDiaryIdAsync(string diaryId)
    {
        var filter = Builders<FoodEntryDocument>.Filter.Eq(x => x.DiaryId, diaryId);
        var documents = await _collection.Find(filter).ToListAsync();

        return FoodEntryMapper.ToDomainList(documents);
    }

    public async Task<int> CountByDiaryIdAsync(string diaryId)
    {
        var filter = Builders<FoodEntryDocument>.Filter.Eq(x => x.DiaryId, diaryId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }
}