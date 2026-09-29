using WebApplication1.AchievementsRewards.Application.Internal;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.shared.infrastructure.Events;
using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Services;

namespace WebApplication1.TreatmentTracking.Application.Internal.Services;

public class TreatmentCommandServiceImpl : ITreatmentCommandService
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IDailyDoseRepository _dailyDoseRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IAchievementRepository _achievementRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly EventPublisher _eventPublisher;
    private readonly ILogger<TreatmentCommandServiceImpl> _logger;

    public TreatmentCommandServiceImpl(
        ITreatmentRepository treatmentRepository,
        IDailyDoseRepository dailyDoseRepository,
        IPatientRepository patientRepository,
        IAchievementRepository achievementRepository,
        IBadgeRepository badgeRepository,
        EventPublisher eventPublisher,
        ILogger<TreatmentCommandServiceImpl> logger)
    {
        _treatmentRepository = treatmentRepository;
        _dailyDoseRepository = dailyDoseRepository;
        _patientRepository = patientRepository;
        _achievementRepository = achievementRepository;
        _badgeRepository = badgeRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<object> StartTreatmentAsync(StartTreatmentCommand command)
    {
        // Validar paciente
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        // Obtener TODOS los tratamientos del paciente
        var existingTreatments = await _treatmentRepository.FindByPatientIdAsync(command.PatientId);

        // Verificar si tiene un tratamiento COMPLETED
        var completedTreatments = existingTreatments.Where(t => t.Status == TreatmentStatus.COMPLETED).ToList();
        if (completedTreatments.Count > 0)
        {
            throw new Exception(
                "Cannot start a new treatment. The patient has already completed a treatment. " +
                "Please discharge the patient if they need a new treatment.");
        }

        // ✅ Verificar y eliminar tratamientos ACTIVOS
        var activeTreatments = existingTreatments.Where(t => t.Status == TreatmentStatus.ACTIVE).ToList();
        foreach (var activeTreatment in activeTreatments) // ← Renombrado
        {
            _logger.LogInformation("[startTreatment] Patient {PatientId} has an ACTIVE treatment. Deleting it...", command.PatientId);
            await DeleteTreatmentCompletelyAsync(activeTreatment.Id);
        }

        // ✅ Verificar y eliminar tratamientos ABANDONADOS
        var abandonedTreatments = existingTreatments.Where(t => t.Status == TreatmentStatus.ABANDONED).ToList();
        foreach (var abandonedTreatment in abandonedTreatments) // ← Renombrado
        {
            _logger.LogInformation("[startTreatment] Patient {PatientId} has an ABANDONED treatment. Deleting it...", command.PatientId);
            await DeleteTreatmentCompletelyAsync(abandonedTreatment.Id);
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(command.DurationDays);

        var initialRisk = new RiskScore(
            Guid.NewGuid().ToString(),
            10,
            RiskLevel.LOW,
            DateTime.UtcNow
        );

        var newTreatment = new Treatment( // ← Renombrado de 'treatment' a 'newTreatment'
            Guid.NewGuid().ToString(),
            command.PatientId,
            command.NurseId,
            command.SupplementName,
            command.Quantity,
            command.DosingHours,
            command.DurationDays,
            startDate,
            endDate,
            TreatmentStatus.ACTIVE,
            100,
            0,
            0,
            0,
            null,
            null,
            initialRisk
        );

        // Generar Daily Doses automáticamente
        var doses = new List<DailyDose>();
        for (int i = 0; i < command.DurationDays; i++)
        {
            var scheduledDate = startDate.AddDays(i);
            doses.Add(new DailyDose(
                Guid.NewGuid().ToString(),
                newTreatment.Id,
                scheduledDate,
                null,
                DoseStatus.PENDING
            ));
        }

        await _treatmentRepository.SaveAsync(newTreatment);
        await _dailyDoseRepository.SaveManyAsync(doses);

        var patientData = patient.ToPrimitives();

        // Publicar evento para Achievements
        await _eventPublisher.PublishAsync("TreatmentStarted", new TreatmentStartedEvent
        {
            TreatmentId = newTreatment.Id,
            PatientId = command.PatientId,
            MotherId = patientData.MotherId,
            DurationDays = command.DurationDays
        });

        return new
        {
            message = "Treatment started successfully",
            treatment = newTreatment.ToPrimitives(),
            totalGeneratedDoses = doses.Count
        };
    }

    public async Task<object> ConfirmDoseAsync(ConfirmDoseCommand command)
    {
        // 1. Validar paciente existe
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        // 2. Validar madre (vs token)
        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != command.MotherId)
        {
            throw new Exception("Mother is not assigned to this patient");
        }

        // 3. Buscar tratamiento activo
        var activeTreatment = await _treatmentRepository.FindActiveByPatientIdAsync(command.PatientId);
        if (activeTreatment == null)
        {
            throw new Exception("Patient does not have an active treatment");
        }

        if (activeTreatment.Status != TreatmentStatus.ACTIVE)
        {
            throw new Exception("Treatment is not active");
        }

        // 4. Buscar dosis PENDIENTE de hoy
        var todayDose = await _dailyDoseRepository.FindTodayDoseAsync(activeTreatment.Id);
        if (todayDose == null)
        {
            throw new Exception("No pending dose found for today");
        }

        if (todayDose.Status != DoseStatus.PENDING)
        {
            throw new Exception("Today's dose is already confirmed or omitted");
        }

        // 5. Confirmar dosis
        todayDose.Confirm();

        // 6. Actualizar adherencia
        activeTreatment.UpdateAdherenceMetrics(true);

        // 7. Recalcular riesgo (baja 10 puntos)
        var risk = activeTreatment.RiskScore;
        var currentScore = Math.Max(0, risk.Score - 10);
        risk.UpdateScore(currentScore);
        activeTreatment.UpdateRiskScore(risk);

        // 8. Persistir
        await _dailyDoseRepository.UpdateAsync(todayDose);
        await _treatmentRepository.UpdateAsync(activeTreatment);

        // 9. Publicar evento para Achievements
        await _eventPublisher.PublishAsync("DailyDoseConfirmed", new DailyDoseConfirmedEvent
        {
            TreatmentId = activeTreatment.Id,
            PatientId = command.PatientId,
            DailyDoseId = todayDose.Id
        });

        return new
        {
            message = "Dose confirmed successfully",
            dose = todayDose.ToPrimitives(),
            treatment = activeTreatment.ToPrimitives()
        };
    }

    public async Task<object> CompleteTreatmentAsync(CompleteTreatmentCommand command)
    {
        var treatment = await _treatmentRepository.FindByIdAsync(command.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        treatment.CompleteTreatment(command.NurseId, command.Observation);

        await _treatmentRepository.UpdateAsync(treatment);

        await _eventPublisher.PublishAsync("TreatmentCompleted", new TreatmentCompletedEvent
        {
            TreatmentId = command.TreatmentId
        });

        return new
        {
            message = "Treatment completed successfully",
            treatment = treatment.ToPrimitives()
        };
    }

    public async Task<object> AbandonTreatmentAsync(AbandonTreatmentCommand command)
    {
        var treatment = await _treatmentRepository.FindByIdAsync(command.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        treatment.AbandonTreatment(command.NurseId, command.Observation);

        await DeleteAllDosesForTreatmentAsync(treatment.Id);
        await DeleteAchievementAndBadgesForTreatmentAsync(treatment.Id);

        await _treatmentRepository.UpdateAsync(treatment);

        await _eventPublisher.PublishAsync("TreatmentAbandoned", new TreatmentAbandonedEvent
        {
            TreatmentId = command.TreatmentId
        });

        return new
        {
            message = "Treatment marked as abandoned successfully. All associated doses, achievements and badges have been removed.",
            treatment = treatment.ToPrimitives()
        };
    }

    public async Task<object> EvaluateMissedDoseAsync(EvaluateMissedDoseCommand command)
    {
        var dose = await _dailyDoseRepository.FindByIdAsync(command.DailyDoseId);
        if (dose == null)
        {
            throw new Exception("Daily dose not found");
        }

        if (dose.Status != DoseStatus.PENDING)
        {
            return new { message = "Dose already processed" };
        }

        var hoursWithoutConfirmation = dose.CalculateHoursWithoutConfirmation();

        if (hoursWithoutConfirmation < 72)
        {
            return new
            {
                message = "Dose still within allowed confirmation window",
                hoursWithoutConfirmation
            };
        }

        dose.MarkAsOmitted();

        var treatment = await _treatmentRepository.FindByIdAsync(dose.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        treatment.UpdateAdherenceMetrics(false);

        var risk = treatment.RiskScore;
        var newScore = Math.Min(100, risk.Score + 20);
        risk.UpdateScore(newScore);
        treatment.UpdateRiskScore(risk);

        await _dailyDoseRepository.UpdateAsync(dose);
        await _treatmentRepository.UpdateAsync(treatment);

        await _eventPublisher.PublishAsync("DailyDoseOmitted", new DailyDoseOmittedEvent
        {
            TreatmentId = treatment.Id,
            DailyDoseId = dose.Id
        });

        return new
        {
            message = "Missed dose evaluated successfully",
            hoursWithoutConfirmation,
            dose = dose.ToPrimitives(),
            treatment = treatment.ToPrimitives()
        };
    }

    // ==========================================
    // MÉTODOS DE PRUEBA
    // ==========================================

    public async Task<object> ForceOmitDoseForTestingAsync(string dailyDoseId)
    {
        var dose = await _dailyDoseRepository.FindByIdAsync(dailyDoseId);
        if (dose == null)
        {
            throw new Exception("Daily dose not found");
        }

        if (dose.Status != DoseStatus.PENDING)
        {
            throw new Exception("Only pending doses can be omitted");
        }

        dose.MarkAsOmitted();

        var treatment = await _treatmentRepository.FindByIdAsync(dose.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        treatment.UpdateAdherenceMetrics(false);

        var risk = treatment.RiskScore;
        var newScore = Math.Min(100, risk.Score + 20);
        risk.UpdateScore(newScore);
        treatment.UpdateRiskScore(risk);

        await _dailyDoseRepository.UpdateAsync(dose);
        await _treatmentRepository.UpdateAsync(treatment);

        await _eventPublisher.PublishAsync("DailyDoseOmitted", new
        {
            treatmentId = treatment.Id,
            dailyDoseId = dose.Id
        });

        return new
        {
            message = "Dose force-omitted for testing",
            dose = dose.ToPrimitives(),
            treatment = treatment.ToPrimitives()
        };
    }

    public async Task<object> ForceConfirmDoseForTestingAsync(string dailyDoseId)
    {
        var dose = await _dailyDoseRepository.FindByIdAsync(dailyDoseId);
        if (dose == null)
        {
            throw new Exception("Daily dose not found");
        }

        if (dose.Status != DoseStatus.PENDING)
        {
            throw new Exception("Dose is already confirmed or omitted");
        }

        var treatment = await _treatmentRepository.FindByIdAsync(dose.TreatmentId);
        if (treatment == null)
        {
            throw new Exception("Treatment not found");
        }

        if (treatment.Status != TreatmentStatus.ACTIVE)
        {
            throw new Exception("Treatment is not active");
        }

        var patientId = treatment.PatientId;
        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        var motherId = patientData.MotherId;

        if (string.IsNullOrEmpty(motherId))
        {
            throw new Exception("Patient has no mother assigned");
        }

        dose.Confirm();
        treatment.UpdateAdherenceMetrics(true);

        var risk = treatment.RiskScore;
        var currentScore = Math.Max(0, risk.Score - 10);
        risk.UpdateScore(currentScore);
        treatment.UpdateRiskScore(risk);

        await _dailyDoseRepository.UpdateAsync(dose);
        await _treatmentRepository.UpdateAsync(treatment);

        await _eventPublisher.PublishAsync("DailyDoseConfirmed", new
        {
            treatmentId = treatment.Id,
            patientId = patientId,
            dailyDoseId = dose.Id,
            motherId = motherId
        });

        return new
        {
            message = "Dose force-confirmed for testing",
            dose = dose.ToPrimitives(),
            treatment = treatment.ToPrimitives()
        };
    }

    // ==========================================
    // MÉTODOS PRIVADOS
    // ==========================================

    private async Task DeleteAllDosesForTreatmentAsync(string treatmentId)
    {
        var allDoses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatmentId);
        if (allDoses.Count == 0) return;

        var doseIds = allDoses.Select(d => d.Id).ToList();
        await _dailyDoseRepository.DeleteManyAsync(doseIds);
    }

    private async Task DeleteAchievementAndBadgesForTreatmentAsync(string treatmentId)
    {
        try
        {
            var achievement = await _achievementRepository.FindByTreatmentIdAsync(treatmentId);
            if (achievement != null)
            {
                await _badgeRepository.DeleteByAchievementIdAsync(achievement.Id);
                await _achievementRepository.DeleteAsync(achievement.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteAchievementAndBadgesForTreatment] Error deleting achievement and badges");
        }
    }

    private async Task DeleteTreatmentCompletelyAsync(string treatmentId)
    {
        try
        {
            _logger.LogInformation("[deleteTreatmentCompletely] Deleting treatment {TreatmentId}...", treatmentId);

            var doses = await _dailyDoseRepository.FindByTreatmentIdAsync(treatmentId);
            var doseIds = doses.Select(d => d.Id).ToList();

            if (doseIds.Count > 0)
            {
                await _dailyDoseRepository.DeleteManyAsync(doseIds);
                _logger.LogInformation("[deleteTreatmentCompletely] Deleted {Count} doses", doseIds.Count);
            }

            await DeleteAchievementAndBadgesForTreatmentAsync(treatmentId);
            await _treatmentRepository.DeleteAsync(treatmentId);

            _logger.LogInformation("[deleteTreatmentCompletely] Treatment {TreatmentId} deleted", treatmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[deleteTreatmentCompletely] Error");
        }
    }
}