using WebApplication1.Consultation.Domain.Models.Queries;

namespace WebApplication1.Consultation.Domain.Servicies;



public interface ICommunicationQueryService
{
    Task<object> GetPatientsWithNurseAssignmentAsync(GetPatientsWithNurseAssignmentQuery query);
    Task<object> GetNurseInfoForConsultationAsync(GetNurseInfoForConsultationQuery query);
    Task<object> GetConsultationChatAsync(GetConsultationChatQuery query);
    Task<object> GetOpenConsultationsByMotherAsync(GetOpenConsultationsByMotherQuery query);
    Task<object> GetOpenConsultationsByNurseAsync(GetOpenConsultationsByNurseQuery query);
    Task<object> GetMessagesAfterAsync(GetMessagesAfterQuery query);
}