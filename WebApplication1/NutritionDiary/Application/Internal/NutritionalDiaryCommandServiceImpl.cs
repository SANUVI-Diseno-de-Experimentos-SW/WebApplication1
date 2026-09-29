using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.NutritionDiary.Domain.Models.Aggregate;
using WebApplication1.NutritionDiary.Domain.Models.Commands;
using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Domain.Models.ValueObjects;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Domain.Services;
using WebApplication1.patient_management.Domain.Repositories;

namespace WebApplication1.NutritionDiary.Application.Internal;


public class NutritionalDiaryCommandServiceImpl : INutritionalDiaryCommandService
{
    private readonly INutritionalDiaryRepository _diaryRepository;
    private readonly IFoodEntryRepository _foodEntryRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IIronCalculatorService _ironCalculator;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NutritionalDiaryCommandServiceImpl> _logger;

    public NutritionalDiaryCommandServiceImpl(
        INutritionalDiaryRepository diaryRepository,
        IFoodEntryRepository foodEntryRepository,
        IFoodItemRepository foodItemRepository,
        IIronCalculatorService ironCalculator,
        IPatientRepository patientRepository,
        IUserRepository userRepository,
        ILogger<NutritionalDiaryCommandServiceImpl> logger)
    {
        _diaryRepository = diaryRepository;
        _foodEntryRepository = foodEntryRepository;
        _foodItemRepository = foodItemRepository;
        _ironCalculator = ironCalculator;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<object> RegisterFoodEntryAsync(RegisterFoodEntryCommand command)
    {
        _logger.LogInformation("[NutritionalDiaryCommandService] registerFoodEntry - INICIO");
        _logger.LogInformation("[NutritionalDiaryCommandService] patientId: {PatientId}", command.PatientId);
        _logger.LogInformation("[NutritionalDiaryCommandService] motherId: {MotherId}", command.MotherId);
        _logger.LogInformation("[NutritionalDiaryCommandService] foodItemId: {FoodItemId}", command.FoodItemId);
        _logger.LogInformation("[NutritionalDiaryCommandService] quantity: {Quantity}", command.Quantity);

        // 1. Validate mother exists
        var mother = await _userRepository.FindMotherByIdAsync(command.MotherId);
        if (mother == null)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Mother not found: {MotherId}", command.MotherId);
            throw new Exception("Mother not found");
        }

        _logger.LogInformation("[NutritionalDiaryCommandService] Mother found: {MotherId}", mother.Id);

        // 2. Validate patient exists
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient == null)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Patient not found: {PatientId}", command.PatientId);
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        _logger.LogInformation("[NutritionalDiaryCommandService] Patient found: {PatientName}", patientData.Name);

        // 3. Validate patient belongs to mother
        if (patientData.MotherId != command.MotherId)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Patient does not belong to mother");
            throw new Exception("This mother is not assigned to this patient");
        }

        // 4. Find food item
        var foodItem = await _foodItemRepository.FindByIdAsync(command.FoodItemId);
        if (foodItem == null)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Food item not found: {FoodItemId}", command.FoodItemId);
            throw new Exception("Food item not found");
        }

        var foodData = foodItem.ToPrimitives();
        _logger.LogInformation("[NutritionalDiaryCommandService] Food item found: {FoodName}", foodData.Name);

        // 5. Determine unit automatically
        var unit = DetermineUnit(foodData.Category, foodData.Name);

        // 6. Find today's diary or create new one
        var diary = await _diaryRepository.FindTodayByPatientIdAsync(command.PatientId);

        if (diary == null)
        {
            _logger.LogInformation("[NutritionalDiaryCommandService] No se encontró diario, creando uno nuevo...");

            var today = DateTime.UtcNow;
            var startOfDay = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);

            _logger.LogInformation("[NutritionalDiaryCommandService] Creando diario con fecha UTC: {Date}", startOfDay);

            diary = new NutritionalDiary(
                Guid.NewGuid().ToString(),
                command.PatientId,
                command.MotherId,
                startOfDay,
                0,
                false
            );

            await _diaryRepository.SaveAsync(diary);
            _logger.LogInformation("[NutritionalDiaryCommandService] Diario creado: {DiaryId}", diary.Id);
        }
        else
        {
            _logger.LogInformation("[NutritionalDiaryCommandService] Diario encontrado: {DiaryId}", diary.Id);
        }

        // 7. Calculate absorbed iron
        var ironAbsorbed = _ironCalculator.CalculateIronAbsorption(
            foodData.NutrientContent.IronMg,
            command.Quantity,
            foodData.NutrientContent.IronType
        );

        _logger.LogInformation("[NutritionalDiaryCommandService] Iron absorbed: {IronAbsorbed}", ironAbsorbed);

        // 8. Create food entry
        var foodEntry = new FoodEntry(
            Guid.NewGuid().ToString(),
            diary.Id,
            command.FoodItemId,
            command.Quantity,
            unit,
            ironAbsorbed,
            DateTime.UtcNow
        );

        await _foodEntryRepository.SaveAsync(foodEntry);
        _logger.LogInformation("[NutritionalDiaryCommandService] Food entry creado: {FoodEntryId}", foodEntry.Id);

        // 9. Update diary totals
        var newTotal = Math.Round(diary.TotalIronAbsorbed + ironAbsorbed, 2);

        string? warningMessage = null;

        if (foodData.IsInhibitor)
        {
            diary.MarkInhibitorDetected();
            warningMessage = $"¡Advertencia! {foodData.Name} puede reducir la absorción del suplemento de hierro.";
        }

        diary.UpdateMetrics(newTotal, diary.HasInhibitor);

        await _diaryRepository.UpdateAsync(diary);
        _logger.LogInformation("[NutritionalDiaryCommandService] Diario actualizado - newTotal: {NewTotal}", newTotal);

        // 10. Response
        var response = new
        {
            success = true,
            message = "Alimento registrado exitosamente",
            foodEntry = new
            {
                id = foodEntry.Id,
                foodName = foodData.Name,
                quantity = command.Quantity,
                unit,
                ironAbsorbed,
                isInhibitor = foodData.IsInhibitor
            },
            newTotalIronAbsorbed = newTotal,
            warningMessage
        };

        _logger.LogInformation("[NutritionalDiaryCommandService] registerFoodEntry - ÉXITO");

        return response;
    }

    public async Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId)
    {
        _logger.LogInformation("[NutritionalDiaryCommandService] validatePatientBelongsToMother - patientId: {PatientId}, motherId: {MotherId}", patientId, motherId);

        var patient = await _patientRepository.FindByIdAsync(patientId);

        if (patient == null)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Paciente no encontrado: {PatientId}", patientId);
            throw new Exception("Paciente no encontrado");
        }

        var patientData = patient.ToPrimitives();

        if (patientData.MotherId != motherId)
        {
            _logger.LogError("[NutritionalDiaryCommandService] Paciente no pertenece a la madre");
            throw new Exception("Este paciente no pertenece a esta madre");
        }

        _logger.LogInformation("[NutritionalDiaryCommandService] Validación exitosa");
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