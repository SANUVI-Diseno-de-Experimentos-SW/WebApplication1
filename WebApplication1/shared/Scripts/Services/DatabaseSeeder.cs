using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
/**
using MongoDB.Driver;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.Services;

/// <summary>
/// Servicio de seed para la base de datos
/// Se ejecuta automáticamente al iniciar la aplicación
/// </summary>
public class DatabaseSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var foodItemCollection = database.GetCollection<FoodItemDocument>("fooditems");

        try
        {
            _logger.LogInformation("🔄 Iniciando seed de alimentos...");

            // Verificar si ya existen datos
            var existingCount = await foodItemCollection.CountDocumentsAsync(
                FilterDefinition<FoodItemDocument>.Empty,
                cancellationToken: cancellationToken
            );

            if (existingCount > 0)
            {
                _logger.LogInformation("✅ Alimentos ya existentes en la base de datos. Encontrados: {Count}", existingCount);
                return;
            }

            // Obtener los alimentos
            var foodItems = GetFoodItems();

            // Insertar en lotes
            var batchSize = 50;
            for (int i = 0; i < foodItems.Count; i += batchSize)
            {
                var batch = foodItems.Skip(i).Take(batchSize);
                await foodItemCollection.InsertManyAsync(batch, cancellationToken: cancellationToken);
                _logger.LogInformation("📦 Lote {BatchNumber} insertado: {Count} items", (i / batchSize) + 1, batch.Count());
            }

            var finalCount = await foodItemCollection.CountDocumentsAsync(
                FilterDefinition<FoodItemDocument>.Empty,
                cancellationToken: cancellationToken
            );
            _logger.LogInformation("✅ Seed completado exitosamente. Insertados: {Count} items", finalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante el seed de la base de datos");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 DatabaseSeeder detenido");
        return Task.CompletedTask;
    }

    private List<FoodItemDocument> GetFoodItems()
    {
        return new List<FoodItemDocument>
        {
            // ==========================================
            // CARNES (MEAT)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_001", 
                Name = "Sangrecita de pollo", 
                NutrientContent = new NutrientContentDocument { IronMg = 29.5, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_002", 
                Name = "Bazo de res", 
                NutrientContent = new NutrientContentDocument { IronMg = 14.0, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_003", 
                Name = "Hígado de pollo", 
                NutrientContent = new NutrientContentDocument { IronMg = 8.5, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_004", 
                Name = "Hígado de res", 
                NutrientContent = new NutrientContentDocument { IronMg = 6.5, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_005", 
                Name = "Carne de res", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.7, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_006", 
                Name = "Pavo", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.8, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_007", 
                Name = "Huevo entero cocido", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.8, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_008", 
                Name = "Pollo", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.3, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "MEAT" 
            },

            // ==========================================
            // PESCADOS (FISH)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_009", 
                Name = "Anchoveta", 
                NutrientContent = new NutrientContentDocument { IronMg = 3.2, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "FISH" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_010", 
                Name = "Sardina en conserva", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.9, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "FISH" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_011", 
                Name = "Atún en conserva", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.9, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "FISH" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_012", 
                Name = "Bonito", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.5, IronType = "hemo" },
                IsInhibitor = false, 
                Category = "FISH" 
            },

            // ==========================================
            // LEGUMBRES (LEGUME)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_013", 
                Name = "Lentejas cocidas", 
                NutrientContent = new NutrientContentDocument { IronMg = 3.3, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "LEGUME" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_014", 
                Name = "Garbanzos cocidos", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.9, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "LEGUME" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_015", 
                Name = "Pallares cocidos", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.5, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "LEGUME" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_016", 
                Name = "Frijoles cocidos", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "LEGUME" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_017", 
                Name = "Arvejas cocidas", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.8, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "LEGUME" 
            },

            // ==========================================
            // VEGETALES (VEGETABLE)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_018", 
                Name = "Espinaca cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.8, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_019", 
                Name = "Acelga cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.8, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_020", 
                Name = "Brócoli cocido", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.7, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_021", 
                Name = "Camote cocido", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.7, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_022", 
                Name = "Papa cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.5, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_023", 
                Name = "Zanahoria cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.4, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_024", 
                Name = "Zapallo cocido", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.4, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "VEGETABLE" 
            },

            // ==========================================
            // CEREALES (GRAIN)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_025", 
                Name = "Kiwicha cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 3.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "GRAIN" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_026", 
                Name = "Pan de trigo", 
                NutrientContent = new NutrientContentDocument { IronMg = 2.5, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "GRAIN" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_027", 
                Name = "Avena cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.7, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "GRAIN" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_028", 
                Name = "Quinua cocida", 
                NutrientContent = new NutrientContentDocument { IronMg = 1.5, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "GRAIN" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_029", 
                Name = "Arroz cocido", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.2, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "GRAIN" 
            },

            // ==========================================
            // FRUTAS (FRUIT)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_030", 
                Name = "Lúcuma", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.4, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_031", 
                Name = "Plátano", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.3, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_032", 
                Name = "Naranja", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_033", 
                Name = "Mandarina", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_034", 
                Name = "Mango", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_035", 
                Name = "Papaya", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "FRUIT" 
            },

            // ==========================================
            // LÁCTEOS (DAIRY) - INHIBIDORES
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_036", 
                Name = "Queso fresco", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.2, IronType = "no-hemo" },
                IsInhibitor = true, 
                Category = "DAIRY" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_037", 
                Name = "Leche de vaca", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = true, 
                Category = "DAIRY" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_038", 
                Name = "Yogur", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = true, 
                Category = "DAIRY" 
            },

            // ==========================================
            // BEBIDAS (BEVERAGE)
            // ==========================================
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_039", 
                Name = "Té", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.0, IronType = "no-hemo" },
                IsInhibitor = true, 
                Category = "BEVERAGE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_040", 
                Name = "Café", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.0, IronType = "no-hemo" },
                IsInhibitor = true, 
                Category = "BEVERAGE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_041", 
                Name = "Jugo de naranja", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.1, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "BEVERAGE" 
            },
            new FoodItemDocument 
            { 
                FoodItemId = "FOOD_042", 
                Name = "Agua", 
                NutrientContent = new NutrientContentDocument { IronMg = 0.0, IronType = "no-hemo" },
                IsInhibitor = false, 
                Category = "BEVERAGE" 
            }
        };
    }
}
**/