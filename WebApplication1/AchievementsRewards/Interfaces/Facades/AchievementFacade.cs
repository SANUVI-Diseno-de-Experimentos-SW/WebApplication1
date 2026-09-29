using WebApplication1.AchievementsRewards.Domain.Model.Queries;
using WebApplication1.AchievementsRewards.Domain.Services;

namespace WebApplication1.AchievementsRewards.Interfaces.Facades;


public class AchievementFacade
{
    private readonly IAchievementQueryService _queryService;
    private readonly IAchievementCommandService _commandService;

    public AchievementFacade(
        IAchievementQueryService queryService,
        IAchievementCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task<object> GetPatientAchievementAsync(GetPatientAchievementQuery query)
    {
        return await _queryService.GetPatientAchievementAsync(query);
    }

    public async Task<object> GetPatientBadgesAsync(GetPatientBadgesQuery query)
    {
        return await _queryService.GetPatientBadgesAsync(query);
    }

    public async Task<object> ForceEvaluateBadgesAsync(string patientId)
    {
        return await _commandService.ForceEvaluateBadgesAsync(patientId);
    }
}