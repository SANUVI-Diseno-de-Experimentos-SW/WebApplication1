using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;
using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.AchievementsRewards.Infrastructure.Mappers;

public static class AchievementMapper
{
    // ✅ ToDomain desde AchievementDocument (fuertemente tipado)
    public static Achievement ToDomain(AchievementDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new Achievement(
            document.AchievementId,
            document.PatientId,
            document.MotherId,
            document.TreatmentId,
            document.DurationDays,
            document.CurrentStreak,
            document.LongestStreak,
            document.BestStreak,
            document.StreakStartDate,
            document.TotalPoints,
            AchievementStatusExtensions.FromString(document.Status)
        );
    }

    // ✅ ToPersistence desde Achievement a AchievementDocument
    public static AchievementDocument ToPersistence(Achievement achievement)
    {
        if (achievement == null)
            throw new ArgumentNullException(nameof(achievement));

        var data = achievement.ToPrimitives();

        return new AchievementDocument
        {
            AchievementId = data.Id,
            PatientId = data.PatientId,
            MotherId = data.MotherId,
            TreatmentId = data.TreatmentId,
            DurationDays = data.DurationDays,
            CurrentStreak = data.CurrentStreak,
            LongestStreak = data.LongestStreak,
            BestStreak = data.BestStreak,
            StreakStartDate = data.StreakStartDate,
            TotalPoints = data.TotalPoints,
            Status = data.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}