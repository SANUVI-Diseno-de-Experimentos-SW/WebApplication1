using WebApplication1.Consultation.Domain.Models.Commands;

namespace WebApplication1.Consultation.Domain.Servicies;

public interface ICommunicationCommandService
{
    Task<object> StartConsultationAsync(StartConsultationCommand command);
    Task<object> AddMessageAsync(AddMessageCommand command);
    Task<object> CloseConsultationAsync(CloseConsultationCommand command);
}