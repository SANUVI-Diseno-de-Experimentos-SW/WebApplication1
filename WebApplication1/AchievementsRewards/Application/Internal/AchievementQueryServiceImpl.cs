using Microsoft.Extensions.Logging;
using WebApplication1.AchievementsRewards.Domain.Model.Queries;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Domain.Services;
using WebApplication1.patient_management.Domain.Repositories;

namespace WebApplication1.AchievementsRewards.Application.Internal;



public class AchievementQueryServiceImpl : IAchievementQueryService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AchievementQueryServiceImpl> _logger;

    public AchievementQueryServiceImpl(
        IAchievementRepository achievementRepository,
        IBadgeRepository badgeRepository,
        IPatientRepository patientRepository,
        ILogger<AchievementQueryServiceImpl> logger)
    {
        _achievementRepository = achievementRepository;
        _badgeRepository = badgeRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    public async Task<object> GetPatientAchievementAsync(GetPatientAchievementQuery query)
    {
        _logger.LogInformation("[GetPatientAchievement] Obteniendo achievement para paciente {PatientId}", query.PatientId);

        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != query.MotherId)
        {
            throw new Exception("Access denied: This patient is not assigned to you");
        }

        var achievement = await _achievementRepository.FindByPatientIdAsync(query.PatientId);
        if (achievement == null)
        {
            return new
            {
                patientId = query.PatientId,
                patientName = $"{patientData.Name} {patientData.LastName}",
                status = "INACTIVE",
                totalPoints = 0,
                currentStreak = 0,
                longestStreak = 0,
                message = "No active treatment found"
            };
        }

        return new
        {
            patientId = achievement.PatientId,
            patientName = $"{patientData.Name} {patientData.LastName}",
            status = achievement.Status.ToStringValue(),
            totalPoints = achievement.TotalPoints,
            currentStreak = achievement.CurrentStreak,
            longestStreak = achievement.LongestStreak
        };
    }

    public async Task<object> GetPatientBadgesAsync(GetPatientBadgesQuery query)
    {
        _logger.LogInformation("[GetPatientBadges] Obteniendo badges para paciente {PatientId}", query.PatientId);

        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != query.MotherId)
        {
            throw new Exception("Access denied: This patient is not assigned to you");
        }

        var achievement = await _achievementRepository.FindByPatientIdAsync(query.PatientId);
        if (achievement == null)
        {
            return new
            {
                patientId = query.PatientId,
                patientName = $"{patientData.Name} {patientData.LastName}",
                badges = new List<object>(),
                message = "No active treatment found"
            };
        }

        var badges = await _badgeRepository.FindByAchievementIdAsync(achievement.Id);
        var currentStreak = achievement.CurrentStreak;
        var durationDays = achievement.DurationDays;

        // Ordenar badges por milestone (menor a mayor)
        var sortedBadges = badges.OrderBy(b => b.Milestone).ToList();

        // Calcular progreso para cada badge
        var badgesWithProgress = sortedBadges.Select(badge =>
        {
            var milestone = badge.Milestone;
            var isUnlocked = badge.IsUnlocked;

            int progress;
            int daysNeeded;

            if (isUnlocked)
            {
                progress = 100;
                daysNeeded = 0;
            }
            else
            {
                // Calcular progreso basado en racha actual vs milestone
                progress = Math.Min(100, (int)Math.Floor((currentStreak / (double)milestone) * 100));
                daysNeeded = Math.Max(0, milestone - currentStreak);
            }

            return new
            {
                id = badge.Id,
                type = badge.Type.ToStringValue(),
                name = badge.Name,
                description = badge.Description,
                milestone = badge.Milestone,
                isUnlocked = isUnlocked,
                unlockedAt = badge.UnlockedAt,
                progress = progress,
                daysNeeded = daysNeeded
            };
        }).ToList();

        return new
        {
            patientId = achievement.PatientId,
            patientName = $"{patientData.Name} {patientData.LastName}",
            badges = badgesWithProgress
        };
    }
}