using WebApplication1.AchievementsRewards.Domain.Model.Queries;

namespace WebApplication1.AchievementsRewards.Domain.Services;

public interface IAchievementQueryService
{
    Task<object> GetPatientAchievementAsync(GetPatientAchievementQuery query);
    Task<object> GetPatientBadgesAsync(GetPatientBadgesQuery query);
}