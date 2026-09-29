using WebApplication1.AchievementsRewards.Domain.Model.Entities;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.AchievementsRewards.Infrastructure.Mappers;

public static class BadgeMapper
{
    // ✅ ToDomain desde BadgeDocument (fuertemente tipado)
    public static Badge ToDomain(BadgeDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new Badge(
            document.BadgeId,
            document.AchievementId,
            BadgeTypeExtensions.FromString(document.Type),
            document.Name,
            document.Description,
            document.Milestone,
            document.IsUnlocked,
            document.UnlockedAt
        );
    }

    // ✅ ToPersistence desde Badge a BadgeDocument
    public static BadgeDocument ToPersistence(Badge badge)
    {
        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        var data = badge.ToPrimitives();

        return new BadgeDocument
        {
            BadgeId = data.Id,
            AchievementId = data.AchievementId,
            Type = data.Type,
            Name = data.Name,
            Description = data.Description,
            Milestone = data.Milestone,
            IsUnlocked = data.IsUnlocked,
            UnlockedAt = data.UnlockedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}