namespace WebApplication1.AchievementsRewards.Domain.Services;

public interface IAchievementCommandService
{
    Task<object> ForceEvaluateBadgesAsync(string patientId);
}