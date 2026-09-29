using WebApplication1.NutritionDiary.Domain.Models.Commands;
using WebApplication1.NutritionDiary.Domain.Models.Queries;
using WebApplication1.NutritionDiary.Domain.Services;

namespace WebApplication1.NutritionDiary.Interfaces.Facade;

public class NutritionalDiaryFacade
{
    private readonly INutritionalDiaryCommandService _commandService;
    private readonly INutritionalDiaryQueryService _queryService;

    public NutritionalDiaryFacade(
        INutritionalDiaryCommandService commandService,
        INutritionalDiaryQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    /// <summary>
    /// Register food consumed by mother
    /// </summary>
    public async Task<object> RegisterFoodEntryAsync(RegisterFoodEntryCommand command)
    {
        return await _commandService.RegisterFoodEntryAsync(command);
    }

    /// <summary>
    /// Get today's nutritional diary
    /// </summary>
    public async Task<object> GetTodayNutritionalDiaryAsync(GetTodayNutritionalDiaryQuery query)
    {
        return await _queryService.GetTodayNutritionalDiaryAsync(query);
    }

    /// <summary>
    /// Get food items by category
    /// </summary>
    public async Task<object> GetFoodItemsByCategoryAsync(GetFoodItemsByCategoryQuery query)
    {
        return await _queryService.GetFoodItemsByCategoryAsync(query);
    }

    /// <summary>
    /// Search food items
    /// </summary>
    public async Task<object> SearchFoodItemsAsync(SearchFoodItemsQuery query)
    {
        return await _queryService.SearchFoodItemsAsync(query);
    }

    /// <summary>
    /// Get food item details
    /// </summary>
    public async Task<object> GetFoodItemDetailsAsync(GetFoodItemDetailsQuery query)
    {
        return await _queryService.GetFoodItemDetailsAsync(query);
    }

    /// <summary>
    /// Get nutritional history
    /// </summary>
    public async Task<object> GetNutritionalHistoryAsync(GetNutritionalHistoryQuery query)
    {
        return await _queryService.GetNutritionalHistoryAsync(query);
    }

    /// <summary>
    /// Get nutritional history with validation
    /// </summary>
    public async Task<object> GetNutritionalHistoryWithValidationAsync(GetNutritionalHistoryQuery query, string motherId)
    {
        // Validar que el paciente pertenece a la madre
        await _commandService.ValidatePatientBelongsToMotherAsync(query.PatientId, motherId);

        return await _queryService.GetNutritionalHistoryAsync(query);
    }

    /// <summary>
    /// Validate patient belongs to mother
    /// </summary>
    public async Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId)
    {
        await _commandService.ValidatePatientBelongsToMotherAsync(patientId, motherId);
    }
}