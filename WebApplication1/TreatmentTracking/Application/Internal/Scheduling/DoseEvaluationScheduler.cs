using WebApplication1.AchievementsRewards.Application.Internal;
using WebApplication1.shared.infrastructure.config;
using WebApplication1.shared.infrastructure.Events;
using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;

namespace WebApplication1.TreatmentTracking.Application.Internal.Scheduling;

public class DoseEvaluationScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DoseEvaluationScheduler> _logger;

    public DoseEvaluationScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<DoseEvaluationScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var thresholdHours = DoseConfig.GetOmissionThresholdHours();
        _logger.LogInformation("[DoseEvaluation] Scheduler iniciado. Umbral: {ThresholdHours} horas ({Minutes} minutos)", 
            thresholdHours, thresholdHours * 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluatePendingDosesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DoseEvaluation] Error en evaluación de dosis");
            }

            // Esperar 1 minuto (equivalente a cron '* * * * *')
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("[DoseEvaluation] Scheduler detenido");
    }

    private async Task EvaluatePendingDosesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DoseEvaluation] Evaluando dosis pendientes... {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var dailyDoseRepository = scope.ServiceProvider.GetRequiredService<IDailyDoseRepository>();
        var treatmentRepository = scope.ServiceProvider.GetRequiredService<ITreatmentRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<EventPublisher>();

        var thresholdHours = DoseConfig.GetOmissionThresholdHours();
        var pendingDoses = await dailyDoseRepository.FindPendingOlderThanHoursAsync((int)thresholdHours);

        if (pendingDoses.Count == 0)
        {
            _logger.LogInformation("[DoseEvaluation] No hay dosis pendientes que superen el umbral");
            return;
        }

        _logger.LogInformation("[DoseEvaluation] Encontradas {Count} dosis para evaluar", pendingDoses.Count);

        var treatmentMap = new Dictionary<string, (List<DailyDose> Doses, Treatment Treatment)>();

        foreach (var dose in pendingDoses)
        {
            var treatmentId = dose.TreatmentId;

            if (!treatmentMap.ContainsKey(treatmentId))
            {
                var treatment = await treatmentRepository.FindByIdAsync(treatmentId);
                if (treatment != null)
                {
                    treatmentMap[treatmentId] = (new List<DailyDose>(), treatment);
                }
            }

            if (treatmentMap.ContainsKey(treatmentId))
            {
                treatmentMap[treatmentId].Doses.Add(dose);
            }
        }

        foreach (var (treatmentId, (doses, treatment)) in treatmentMap)
        {
            _logger.LogInformation("[DoseEvaluation] Procesando tratamiento {TreatmentId}, {Count} dosis", treatmentId, doses.Count);

            foreach (var dose in doses)
            {
                if (dose.Status == DoseStatus.PENDING)
                {
                    var hoursOverdue = dose.CalculateHoursWithoutConfirmation();

                    _logger.LogInformation("[DoseEvaluation] Omitiendo dosis {DoseId} con {Hours} horas de retraso", 
                        dose.Id, hoursOverdue);

                    // Marcar como omitida
                    dose.MarkAsOmitted();
                    await dailyDoseRepository.UpdateAsync(dose);

                    // Publicar evento para Achievements
                    await eventPublisher.PublishAsync("DailyDoseOmitted", new DailyDoseOmittedEvent
                    {
                        TreatmentId = treatment.Id,
                        DailyDoseId = dose.Id
                    });

                    // Actualizar métricas del tratamiento
                    treatment.UpdateAdherenceMetrics(false);

                    // Aumentar riesgo +20 puntos
                    var risk = treatment.RiskScore;
                    var newScore = Math.Min(100, risk.Score + 20);
                    risk.UpdateScore(newScore);
                    treatment.UpdateRiskScore(risk);
                }
            }

            await treatmentRepository.UpdateAsync(treatment);
        }

        _logger.LogInformation("[DoseEvaluation] Procesamiento completado. {Count} dosis omitidas", pendingDoses.Count);
    }

    /// <summary>
    /// Ejecuta una evaluación única (para pruebas)
    /// </summary>
    public async Task<object> EvaluateOnceAsync()
    {
        await EvaluatePendingDosesAsync(CancellationToken.None);
        return new { message = "Evaluación manual completada" };
    }
}