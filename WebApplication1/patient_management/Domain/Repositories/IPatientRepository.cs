using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;

namespace WebApplication1.patient_management.Domain.Repositories;



/// <summary>
/// Repositorio de pacientes
/// </summary>
public interface IPatientRepository
{
    // ==========================================
    // CRUD BÁSICO
    // ==========================================

    Task<Patient> SaveAsync(Patient patient);
    Task<Patient?> FindByIdAsync(string id);

    // ==========================================
    // BÚSQUEDAS ESPECÍFICAS
    // ==========================================

    Task<IReadOnlyList<Patient>> FindByMotherIdAsync(string motherId);
    Task<IReadOnlyList<Patient>> FindByNurseIdAsync(string nurseId);

    // ==========================================
    // MÉTODOS PARA ENFERMERAS
    // ==========================================

    Task<IReadOnlyList<Patient>> FindPatientsEligibleForDischargeAsync(string nurseId, string? searchTerm = null);
    Task<int> CountActivePatientsByNurseAsync(string nurseId);

    // ==========================================
    // Actualizar
    // ==========================================
    Task<Patient> UpdateAsync(Patient patient);

    Task<IReadOnlyList<Patient>> FindPatientsAssignedToNurseAsync(string nurseId, string? searchTerm = null);
}