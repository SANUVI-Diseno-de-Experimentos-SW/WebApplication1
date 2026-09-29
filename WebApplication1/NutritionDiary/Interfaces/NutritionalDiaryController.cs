using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.NutritionDiary.Domain.Models.Commands;
using WebApplication1.NutritionDiary.Interfaces.Assemblers;
using WebApplication1.NutritionDiary.Interfaces.Facade;
using WebApplication1.shared.Attributes;

namespace WebApplication1.NutritionDiary.Interfaces;



[ApiController]
[Route("api/nutritional-diary")]
[Authorize]
public class NutritionalDiaryController : ControllerBase
{
    private readonly NutritionalDiaryFacade _facade;

    public NutritionalDiaryController(NutritionalDiaryFacade facade)
    {
        _facade = facade;
    }

    // ==========================================
    // 1. REGISTRAR ALIMENTO - SOLO MADRE
    // ==========================================

    [HttpPost("food-entry")]
    [RequireRole("Mother")]
    public async Task<IActionResult> RegisterFoodEntry([FromBody] RegisterFoodEntryRequest request)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            if (string.IsNullOrEmpty(request.PatientId) || string.IsNullOrEmpty(request.FoodItemId) || request.Quantity <= 0)
            {
                return BadRequest(new { error = "Faltan campos requeridos: patientId, foodItemId, quantity" });
            }

            var command = new RegisterFoodEntryCommand(
                request.PatientId,
                motherId,
                request.FoodItemId,
                request.Quantity
            );

            var result = await _facade.RegisterFoodEntryAsync(command);

            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 2. OBTENER DIARIO DE HOY - SOLO MADRE
    // ==========================================

    [HttpGet("today/{patientId}")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetTodayDiary(string patientId, [FromQuery] string? date = null)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest(new { error = "Patient ID es requerido" });
            }

            await _facade.ValidatePatientBelongsToMotherAsync(patientId, motherId);

            var query = GetTodayNutritionalDiaryQueryAssembler.ToQuery(patientId, date);
            var result = await _facade.GetTodayNutritionalDiaryAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 3. OBTENER ALIMENTOS POR CATEGORÍA - PUBLICO
    // ==========================================

    [HttpGet("foods/category/{category}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFoodsByCategory(string category)
    {
        try
        {
            var query = GetFoodItemsByCategoryQueryAssembler.ToQuery(category);
            var result = await _facade.GetFoodItemsByCategoryAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 4. BUSCAR ALIMENTOS - PUBLICO
    // ==========================================

    [HttpGet("foods/search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchFoods([FromQuery] string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new { error = "Search text is required" });
            }

            var query = SearchFoodItemsQueryAssembler.ToQuery(text);
            var result = await _facade.SearchFoodItemsAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 5. OBTENER DETALLE DE ALIMENTO - PUBLICO
    // ==========================================

    [HttpGet("foods/{foodItemId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFoodDetails(string foodItemId)
    {
        try
        {
            if (string.IsNullOrEmpty(foodItemId))
            {
                return BadRequest(new { error = "Food item ID is required" });
            }

            var query = GetFoodItemDetailsQueryAssembler.ToQuery(foodItemId);
            var result = await _facade.GetFoodItemDetailsAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 6. OBTENER HISTORIAL NUTRICIONAL - SOLO MADRE
    // ==========================================

    [HttpGet("history/{patientId}")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetNutritionalHistory(string patientId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest(new { error = "Patient ID es requerido" });
            }

            var query = GetNutritionalHistoryQueryAssembler.ToQuery(patientId, startDate, endDate);
            var result = await _facade.GetNutritionalHistoryWithValidationAsync(query, motherId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// ==========================================
// REQUEST DTO
// ==========================================

public class RegisterFoodEntryRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string FoodItemId { get; set; } = string.Empty;
    public double Quantity { get; set; }
}