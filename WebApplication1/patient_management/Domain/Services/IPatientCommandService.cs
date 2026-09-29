using WebApplication1.Contexts.PatientManagement.Domain.Commands;
using WebApplication1.patient_management.Domain.Commands;

namespace WebApplication1.patient_management.Domain.Services;

/// <summary>
/// Servicio de comandos para pacientes
/// </summary>
public interface IPatientCommandService
{
    Task RegisterPatientAsync(RegisterPatientCommand command);
    Task AssignPatientToNurseAsync(AssignPatientToNurseCommand command);
    Task DischargePatientAsync(DischargePatientCommand command);
    Task RegisterHemoglobinControlAsync(RegisterHemoglobinControlCommand command);
    Task CreateInitialMedicalRecordAsync(CreateInitialMedicalRecordCommand command);
    Task UpdateMedicalRecordAsync(UpdateMedicalRecordCommand command);
}