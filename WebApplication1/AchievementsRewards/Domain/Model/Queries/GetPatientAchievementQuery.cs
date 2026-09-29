namespace WebApplication1.AchievementsRewards.Domain.Model.Queries;

public record GetPatientAchievementQuery(
    string PatientId,
    string MotherId
);