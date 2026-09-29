using WebApplication1.AchievementsRewards.Domain.Model.Entities;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;

namespace WebApplication1.AchievementsRewards.Domain.Repositories;

public interface IBadgeRepository
{
    Task SaveAsync(Badge badge);
    Task SaveManyAsync(List<Badge> badges);
    Task UpdateAsync(Badge badge);
    Task UpdateManyAsync(List<Badge> badges);
    Task<Badge?> FindByIdAsync(string id);
    Task<List<Badge>> FindByAchievementIdAsync(string achievementId);
    Task<Badge?> FindByAchievementIdAndTypeAsync(string achievementId, BadgeType type);
    Task DeleteByAchievementIdAsync(string achievementId);
}