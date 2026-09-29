using Microsoft.Extensions.Logging;
using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;
using WebApplication1.AchievementsRewards.Domain.Model.Entities;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Domain.Services;

namespace WebApplication1.AchievementsRewards.Application.Internal;




public class TreatmentEventHandlers
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly AchievementEvaluatorService _evaluatorService;
    private readonly ILogger<TreatmentEventHandlers> _logger;

    public TreatmentEventHandlers(
        IAchievementRepository achievementRepository,
        IBadgeRepository badgeRepository,
        ILogger<TreatmentEventHandlers> logger)
    {
        _achievementRepository = achievementRepository;
        _badgeRepository = badgeRepository;
        _logger = logger;
        _evaluatorService = new AchievementEvaluatorService();
    }

    /// <summary>
    /// Cuando se inicia un tratamiento: crear Achievement y Badges
    /// </summary>
    public async Task OnTreatmentStartedAsync(TreatmentStartedEvent eventData)
    {
        try
        {
            _logger.LogInformation("[ACHIEVEMENTS] 🔥 Iniciando OnTreatmentStartedAsync");
            _logger.LogInformation("[ACHIEVEMENTS] TreatmentId: {TreatmentId}", eventData.TreatmentId);
            _logger.LogInformation("[ACHIEVEMENTS] PatientId: {PatientId}", eventData.PatientId);
            _logger.LogInformation("[ACHIEVEMENTS] DurationDays: {DurationDays}", eventData.DurationDays);

            // Verificar si ya existe
            var existing = await _achievementRepository.FindByTreatmentIdAsync(eventData.TreatmentId);
            if (existing != null)
            {
                _logger.LogInformation("[ACHIEVEMENTS] ✅ Achievement ya existe para {TreatmentId}", eventData.TreatmentId);
                return;
            }

            _logger.LogInformation("[ACHIEVEMENTS] Creando nuevo Achievement...");

            // Crear Achievement
            var achievementId = Guid.NewGuid().ToString();
            var achievement = Achievement.Create(
                achievementId,
                eventData.PatientId,
                eventData.MotherId,
                eventData.TreatmentId,
                eventData.DurationDays
            );

            await _achievementRepository.SaveAsync(achievement);
            _logger.LogInformation("[ACHIEVEMENTS] ✅ Achievement creado: {AchievementId}", achievementId);

            // Crear Badges
            var badgeTypes = MilestoneCalculator.GetBadgesForDuration(eventData.DurationDays);
            _logger.LogInformation("[ACHIEVEMENTS] Tipos de badges a crear: {Count}", badgeTypes.Count);
            
            var badges = new List<Badge>();

            foreach (var type in badgeTypes)
            {
                var badge = Badge.Create(
                    Guid.NewGuid().ToString(),
                    achievementId,
                    type,
                    eventData.DurationDays
                );
                badges.Add(badge);
                _logger.LogInformation("[ACHIEVEMENTS] Badge creado: {Type} - Milestone: {Milestone}", 
                    type.ToStringValue(), badge.Milestone);
            }

            await _badgeRepository.SaveManyAsync(badges);
            _logger.LogInformation("[ACHIEVEMENTS] ✅ {Count} badges creados", badges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ACHIEVEMENTS] ❌ ERROR en OnTreatmentStartedAsync");
            throw;
        }
    }
    /// <summary>
    /// Cuando se confirma una dosis: actualizar racha y puntos
    /// </summary>
    public async Task OnDailyDoseConfirmedAsync(DailyDoseConfirmedEvent eventData)
    {
        _logger.LogInformation("[Achievements] DailyDoseConfirmed event received: {TreatmentId}", eventData.TreatmentId);

        var achievement = await _achievementRepository.FindByTreatmentIdAsync(eventData.TreatmentId);
        if (achievement == null)
        {
            _logger.LogError("[Achievements] Achievement not found for treatment {TreatmentId}", eventData.TreatmentId);
            return;
        }

        if (achievement.Status != AchievementStatus.ACTIVE)
        {
            _logger.LogInformation("[Achievements] Achievement is not active, skipping");
            return;
        }

        var previousStreak = achievement.CurrentStreak;

        // Actualizar achievement
        achievement.OnDoseConfirmed();
        await _achievementRepository.UpdateAsync(achievement);

        // Evaluar hitos de racha
        var streakEvent = _evaluatorService.EvaluateStreakMilestone(
            achievement,
            previousStreak,
            achievement.CurrentStreak
        );

        if (streakEvent != null)
        {
            _logger.LogInformation("[Achievements] Streak milestone reached: {Milestone} days", streakEvent.Milestone);
        }

        // Evaluar badges desbloqueadas
        var badges = await _badgeRepository.FindByAchievementIdAsync(achievement.Id);
        var (updatedBadges, events) = _evaluatorService.EvaluateBadges(achievement, badges);

        if (updatedBadges.Count > 0)
        {
            await _badgeRepository.UpdateManyAsync(updatedBadges);
            _logger.LogInformation("[Achievements] {Count} new badges unlocked", updatedBadges.Count);

            foreach (var evt in events)
            {
                _logger.LogInformation("[Achievements] Badge unlocked: {BadgeName}", evt.BadgeName);
            }
        }

        _logger.LogInformation("[Achievements] Points: +10, total: {TotalPoints}", achievement.TotalPoints);
    }

    /// <summary>
    /// Cuando se omite una dosis: reiniciar racha
    /// </summary>
    public async Task OnDailyDoseOmittedAsync(DailyDoseOmittedEvent eventData)
    {
        _logger.LogInformation("[Achievements] DailyDoseOmitted event received: {TreatmentId}", eventData.TreatmentId);

        var achievement = await _achievementRepository.FindByTreatmentIdAsync(eventData.TreatmentId);
        if (achievement == null)
        {
            _logger.LogError("[Achievements] Achievement not found for treatment {TreatmentId}", eventData.TreatmentId);
            return;
        }

        if (achievement.Status != AchievementStatus.ACTIVE)
        {
            _logger.LogInformation("[Achievements] Achievement is not active, skipping");
            return;
        }

        achievement.OnDoseOmitted();
        await _achievementRepository.UpdateAsync(achievement);

        _logger.LogInformation("[Achievements] Streak reset to 0 for achievement {AchievementId}", achievement.Id);
    }

    /// <summary>
    /// Cuando se completa un tratamiento: marcar como COMPLETED y dar bonus
    /// </summary>
    public async Task OnTreatmentCompletedAsync(TreatmentCompletedEvent eventData)
    {
        _logger.LogInformation("[Achievements] TreatmentCompleted event received: {TreatmentId}", eventData.TreatmentId);

        var achievement = await _achievementRepository.FindByTreatmentIdAsync(eventData.TreatmentId);
        if (achievement == null)
        {
            _logger.LogError("[Achievements] Achievement not found for treatment {TreatmentId}", eventData.TreatmentId);
            return;
        }

        achievement.OnTreatmentCompleted();
        await _achievementRepository.UpdateAsync(achievement);

        // Asegurar que TREATMENT_COMPLETED badge esté desbloqueada
        var badges = await _badgeRepository.FindByAchievementIdAsync(achievement.Id);
        var treatmentCompletedBadge = badges.FirstOrDefault(b => b.Type == BadgeType.TREATMENT_COMPLETED);

        if (treatmentCompletedBadge != null && !treatmentCompletedBadge.IsUnlocked)
        {
            treatmentCompletedBadge.Unlock();
            await _badgeRepository.UpdateAsync(treatmentCompletedBadge);
            _logger.LogInformation("[Achievements] TREATMENT_COMPLETED badge unlocked");
        }

        _logger.LogInformation("[Achievements] Achievement marked as COMPLETED, bonus +50 points, total: {TotalPoints}", achievement.TotalPoints);
    }

    /// <summary>
    /// Cuando se abandona un tratamiento: marcar como ABANDONED
    /// </summary>
    public async Task OnTreatmentAbandonedAsync(TreatmentAbandonedEvent eventData)
    {
        _logger.LogInformation("[Achievements] TreatmentAbandoned event received: {TreatmentId}", eventData.TreatmentId);

        var achievement = await _achievementRepository.FindByTreatmentIdAsync(eventData.TreatmentId);
        if (achievement == null)
        {
            _logger.LogError("[Achievements] Achievement not found for treatment {TreatmentId}", eventData.TreatmentId);
            return;
        }

        achievement.OnTreatmentAbandoned();
        await _achievementRepository.UpdateAsync(achievement);

        _logger.LogInformation("[Achievements] Achievement marked as ABANDONED");
    }
}

// ==========================================
// EVENTOS DE TRATAMIENTO (DTOs)
// ==========================================

public class TreatmentStartedEvent
{
    public string TreatmentId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public int DurationDays { get; set; }
}

public class DailyDoseConfirmedEvent
{
    public string TreatmentId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string DailyDoseId { get; set; } = string.Empty;
}

public class DailyDoseOmittedEvent
{
    public string TreatmentId { get; set; } = string.Empty;
    public string DailyDoseId { get; set; } = string.Empty;
}

public class TreatmentCompletedEvent
{
    public string TreatmentId { get; set; } = string.Empty;
}

public class TreatmentAbandonedEvent
{
    public string TreatmentId { get; set; } = string.Empty;
}