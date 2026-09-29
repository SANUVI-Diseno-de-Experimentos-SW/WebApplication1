namespace WebApplication1.AchievementsRewards.Domain.Model.Queries;

public record GetPatientBadgesQuery(
    string PatientId,
    string MotherId
);