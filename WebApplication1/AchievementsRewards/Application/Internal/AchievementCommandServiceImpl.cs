using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Domain.Services;

namespace WebApplication1.AchievementsRewards.Application.Internal;

using Microsoft.Extensions.Logging;

public class AchievementCommandServiceImpl : IAchievementCommandService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly AchievementEvaluatorService _evaluatorService;
    private readonly ILogger<AchievementCommandServiceImpl> _logger;

    public AchievementCommandServiceImpl(
        IAchievementRepository achievementRepository,
        IBadgeRepository badgeRepository,
        ILogger<AchievementCommandServiceImpl> logger)
    {
        _achievementRepository = achievementRepository;
        _badgeRepository = badgeRepository;
        _logger = logger;
        _evaluatorService = new AchievementEvaluatorService();
    }

    /// <summary>
    /// [SOLO PRUEBAS] Fuerza la evaluación de badges para un paciente
    /// </summary>
    public async Task<object> ForceEvaluateBadgesAsync(string patientId)
    {
        _logger.LogInformation("[ForceEvaluateBadges] Forzando evaluación para paciente {PatientId}", patientId);

        // Buscar achievement por patientId
        var achievement = await _achievementRepository.FindByPatientIdAsync(patientId);
        if (achievement == null)
        {
            throw new Exception("Achievement not found for this patient");
        }

        // Obtener todas las badges del achievement
        var badges = await _badgeRepository.FindByAchievementIdAsync(achievement.Id);

        // Evaluar qué badges se pueden desbloquear
        var (updatedBadges, events) = _evaluatorService.EvaluateBadges(achievement, badges);

        // Guardar cambios
        if (updatedBadges.Count > 0)
        {
            await _badgeRepository.UpdateManyAsync(updatedBadges);
        }

        _logger.LogInformation("[ForceEvaluateBadges] {Count} badges desbloqueadas", updatedBadges.Count);

        return new
        {
            message = $"{updatedBadges.Count} badges unlocked",
            unlockedBadges = updatedBadges.Select(b => new
            {
                id = b.Id,
                type = b.Type.ToStringValue(),
                name = b.Name,
                unlockedAt = b.UnlockedAt
            }),
            events = events.Select(e => e.ToPrimitives())
        };
    }
}