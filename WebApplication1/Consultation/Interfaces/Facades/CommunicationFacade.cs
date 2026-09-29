using WebApplication1.Consultation.Domain.Models.Commands;
using WebApplication1.Consultation.Domain.Models.Queries;
using WebApplication1.Consultation.Domain.Servicies;

namespace WebApplication1.Consultation.Interfaces.Facades;



public class CommunicationFacade
{
    private readonly ICommunicationCommandService _commandService;
    private readonly ICommunicationQueryService _queryService;

    public CommunicationFacade(
        ICommunicationCommandService commandService,
        ICommunicationQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    public async Task<object> StartConsultationAsync(StartConsultationCommand command)
    {
        return await _commandService.StartConsultationAsync(command);
    }

    public async Task<object> AddMessageAsync(AddMessageCommand command)
    {
        return await _commandService.AddMessageAsync(command);
    }

    public async Task<object> CloseConsultationAsync(CloseConsultationCommand command)
    {
        return await _commandService.CloseConsultationAsync(command);
    }

    public async Task<object> GetPatientsWithNurseAssignmentAsync(GetPatientsWithNurseAssignmentQuery query)
    {
        return await _queryService.GetPatientsWithNurseAssignmentAsync(query);
    }

    public async Task<object> GetNurseInfoForConsultationAsync(GetNurseInfoForConsultationQuery query)
    {
        return await _queryService.GetNurseInfoForConsultationAsync(query);
    }

    public async Task<object> GetConsultationChatAsync(GetConsultationChatQuery query)
    {
        return await _queryService.GetConsultationChatAsync(query);
    }

    public async Task<object> GetOpenConsultationsByMotherAsync(GetOpenConsultationsByMotherQuery query)
    {
        return await _queryService.GetOpenConsultationsByMotherAsync(query);
    }

    public async Task<object> GetOpenConsultationsByNurseAsync(GetOpenConsultationsByNurseQuery query)
    {
        return await _queryService.GetOpenConsultationsByNurseAsync(query);
    }

    public async Task<object> GetMessagesAfterAsync(GetMessagesAfterQuery query)
    {
        return await _queryService.GetMessagesAfterAsync(query);
    }
}