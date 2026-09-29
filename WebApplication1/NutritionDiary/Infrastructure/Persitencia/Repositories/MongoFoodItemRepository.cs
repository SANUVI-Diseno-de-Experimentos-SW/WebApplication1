using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Infrastructure.Mappers;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Repositories;

public class MongoFoodItemRepository : IFoodItemRepository
{
    private readonly IMongoCollection<FoodItemDocument> _collection;
    private readonly ILogger<MongoFoodItemRepository> _logger;

    public MongoFoodItemRepository(IMongoDatabase database, ILogger<MongoFoodItemRepository> logger)
    {
        _collection = database.GetCollection<FoodItemDocument>("fooditems");
        _logger = logger;
    }

    public async Task<FoodItem?> FindByIdAsync(string foodItemId)
    {
        var filter = Builders<FoodItemDocument>.Filter.Eq(x => x.FoodItemId, foodItemId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return FoodItemMapper.ToDomain(document);
    }

    public async Task<List<FoodItem>> FindByCategoryAsync(string category)
    {
        var filter = Builders<FoodItemDocument>.Filter.Eq(x => x.Category, category);
        var documents = await _collection.Find(filter).ToListAsync();

        return FoodItemMapper.ToDomainList(documents);
    }

    public async Task<List<FoodItem>> SearchByNameAsync(string searchText)
    {
        var filter = Builders<FoodItemDocument>.Filter.Regex(x => x.Name, new BsonRegularExpression(searchText, "i"));
        var documents = await _collection.Find(filter).ToListAsync();

        return FoodItemMapper.ToDomainList(documents);
    }
}