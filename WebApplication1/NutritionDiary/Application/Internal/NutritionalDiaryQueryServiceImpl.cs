using WebApplication1.NutritionDiary.Domain.Models.Aggregate;
using WebApplication1.NutritionDiary.Domain.Models.Queries;
using WebApplication1.NutritionDiary.Domain.Models.ValueObjects;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Domain.Services;

namespace WebApplication1.NutritionDiary.Application.Internal;


public class NutritionalDiaryQueryServiceImpl : INutritionalDiaryQueryService
{
    private readonly INutritionalDiaryRepository _diaryRepository;
    private readonly IFoodEntryRepository _foodEntryRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ILogger<NutritionalDiaryQueryServiceImpl> _logger;

    public NutritionalDiaryQueryServiceImpl(
        INutritionalDiaryRepository diaryRepository,
        IFoodEntryRepository foodEntryRepository,
        IFoodItemRepository foodItemRepository,
        ILogger<NutritionalDiaryQueryServiceImpl> logger)
    {
        _diaryRepository = diaryRepository;
        _foodEntryRepository = foodEntryRepository;
        _foodItemRepository = foodItemRepository;
        _logger = logger;
    }

    public async Task<object> GetFoodItemsByCategoryAsync(GetFoodItemsByCategoryQuery query)
    {
        var items = await _foodItemRepository.FindByCategoryAsync(query.Category);

        var sortedItems = items.OrderBy(i => i.Name).ToList();

        return new
        {
            category = query.Category,
            items = sortedItems.Select(item =>
            {
                var data = item.ToPrimitives();
                return new
                {
                    foodItemId = data.Id,
                    name = data.Name,
                    ironType = data.NutrientContent.IronType,
                    ironMgPer100g = data.NutrientContent.IronMg,
                    isInhibitor = data.IsInhibitor
                };
            })
        };
    }

    public async Task<object> SearchFoodItemsAsync(SearchFoodItemsQuery query)
    {
        if (query.SearchText.Trim().Length < 2)
        {
            return new
            {
                searchText = query.SearchText,
                resultCount = 0,
                items = new List<object>()
            };
        }

        var items = await _foodItemRepository.SearchByNameAsync(query.SearchText);

        return new
        {
            searchText = query.SearchText,
            resultCount = items.Count,
            items = items.Select(item =>
            {
                var data = item.ToPrimitives();
                return new
                {
                    foodItemId = data.Id,
                    name = data.Name,
                    ironType = data.NutrientContent.IronType,
                    ironMgPer100g = data.NutrientContent.IronMg,
                    isInhibitor = data.IsInhibitor
                };
            })
        };
    }

    public async Task<object> GetFoodItemDetailsAsync(GetFoodItemDetailsQuery query)
    {
        var foodItem = await _foodItemRepository.FindByIdAsync(query.FoodItemId);

        if (foodItem == null)
            throw new Exception("Food item not found");

        var data = foodItem.ToPrimitives();

        string? warningMessage = null;

        if (data.IsInhibitor)
        {
            warningMessage = $"¡Advertencia! {data.Name} puede reducir la absorción del suplemento de hierro.";
        }

        return new
        {
            foodItemId = data.Id,
            name = data.Name,
            ironType = data.NutrientContent.IronType,
            ironMgPer100g = data.NutrientContent.IronMg,
            isInhibitor = data.IsInhibitor,
            warningMessage,
            defaultUnit = DetermineUnit(data.Category, data.Name)
        };
    }

    public async Task<object> GetNutritionalHistoryAsync(GetNutritionalHistoryQuery query)
    {
        var endDate = query.EndDate ?? DateTime.UtcNow;
        var startDate = query.StartDate ?? endDate.AddDays(-30);

        _logger.LogInformation("[NutritionalDiaryQueryService] getNutritionalHistory - patientId: {PatientId}", query.PatientId);
        _logger.LogInformation("[NutritionalDiaryQueryService] startDate: {StartDate}", startDate);
        _logger.LogInformation("[NutritionalDiaryQueryService] endDate: {EndDate}", endDate);

        var diaries = await _diaryRepository.FindByPatientAndDateRangeAsync(query.PatientId, startDate, endDate);

        var sortedDiaries = diaries.OrderByDescending(d => d.Date).ToList();

        var days = new List<object>();

        foreach (var diary in sortedDiaries)
        {
            var diaryData = diary.ToPrimitives();

            var entries = await _foodEntryRepository.FindByDiaryIdAsync(diaryData.Id);

            var inhibitorCount = 0;

            foreach (var entry in entries)
            {
                try
                {
                    var foodItem = await _foodItemRepository.FindByIdAsync(entry.FoodItemId);
                    if (foodItem != null && foodItem.ToPrimitives().IsInhibitor)
                    {
                        inhibitorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando entry");
                }
            }

            days.Add(new
            {
                date = diaryData.Date,
                displayDate = diaryData.Date.ToString("dd 'de' MMMM", new System.Globalization.CultureInfo("es-PE")),
                totalIronAbsorbed = Math.Round(diaryData.TotalIronAbsorbed, 1),
                hasInhibitor = diaryData.HasInhibitor,
                inhibitorCount,
                totalFoodEntries = entries.Count
            });
        }

        return new
        {
            patientId = query.PatientId,
            period = new { startDate, endDate },
            days
        };
    }

    public async Task<object> GetTodayNutritionalDiaryAsync(GetTodayNutritionalDiaryQuery query)
    {
        _logger.LogInformation("[NutritionalDiaryQueryService] getTodayNutritionalDiary - patientId: {PatientId}", query.PatientId);
        _logger.LogInformation("[NutritionalDiaryQueryService] date param: {Date}", query.Date ?? "no especificada");

        NutritionalDiary? diary = null;

        // Si se envió una fecha, buscar por esa fecha
        if (!string.IsNullOrEmpty(query.Date))
        {
            _logger.LogInformation("[NutritionalDiaryQueryService] Buscando diario con fecha específica: {Date}", query.Date);

            var parts = query.Date.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[0], out var year) &&
                int.TryParse(parts[1], out var month) && int.TryParse(parts[2], out var day))
            {
                var startOfDay = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                var endOfDay = new DateTime(year, month, day, 23, 59, 59, 999, DateTimeKind.Utc);

                var diaries = await _diaryRepository.FindByPatientAndDateRangeAsync(query.PatientId, startOfDay, endOfDay);
                diary = diaries.Count > 0 ? diaries[0] : null;
            }
        }
        else
        {
            // Si no hay fecha, usar el método estándar
            diary = await _diaryRepository.FindTodayByPatientIdAsync(query.PatientId);
        }

        if (diary == null)
        {
            _logger.LogInformation("[NutritionalDiaryQueryService] No se encontró diario para patientId: {PatientId}", query.PatientId);
            return new
            {
                diaryId = (string?)null,
                date = DateTime.UtcNow,
                totalIronAbsorbed = 0,
                foodEntries = new List<object>()
            };
        }

        var diaryData = diary.ToPrimitives();

        _logger.LogInformation("[NutritionalDiaryQueryService] Diario encontrado: {DiaryId}", diaryData.Id);
        _logger.LogInformation("[NutritionalDiaryQueryService] Fecha del diario: {Date}", diaryData.Date);

        var entries = await _foodEntryRepository.FindByDiaryIdAsync(diaryData.Id);

        if (entries == null || entries.Count == 0)
        {
            return new
            {
                diaryId = diaryData.Id,
                date = diaryData.Date,
                totalIronAbsorbed = Math.Round(diaryData.TotalIronAbsorbed, 2),
                foodEntries = new List<object>()
            };
        }

        var enrichedEntries = new List<object>();

        foreach (var entry in entries)
        {
            try
            {
                var foodItem = await _foodItemRepository.FindByIdAsync(entry.FoodItemId);
                var foodData = foodItem?.ToPrimitives();

                enrichedEntries.Add(new
                {
                    entryId = entry.Id,
                    foodName = foodData?.Name ?? "Desconocido",
                    quantity = entry.Quantity,
                    unit = entry.Unit,
                    ironAbsorbed = entry.IronContributed,
                    isInhibitor = foodData?.IsInhibitor ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando entry");
                enrichedEntries.Add(new
                {
                    entryId = "error",
                    foodName = "Error al procesar",
                    quantity = 0,
                    unit = "",
                    ironAbsorbed = 0,
                    isInhibitor = false
                });
            }
        }

        var response = new
        {
            diaryId = diaryData.Id,
            date = diaryData.Date,
            totalIronAbsorbed = Math.Round(diaryData.TotalIronAbsorbed, 2),
            foodEntries = enrichedEntries
        };

        _logger.LogInformation("[NutritionalDiaryQueryService] getTodayNutritionalDiary - ÉXITO");
        _logger.LogInformation("[NutritionalDiaryQueryService] Alimentos: {Count}, totalFe: {Total}", enrichedEntries.Count, response.totalIronAbsorbed);

        return response;
    }

    private string DetermineUnit(string category, string foodName)
    {
        var normalizedName = foodName.ToLowerInvariant();

        if (category == FoodCategory.BEVERAGE.ToStringValue())
            return "mililitros";

        if (category == FoodCategory.DAIRY.ToStringValue() &&
            (normalizedName.Contains("leche") || normalizedName.Contains("yogur")))
            return "mililitros";

        return "gramos";
    }
}