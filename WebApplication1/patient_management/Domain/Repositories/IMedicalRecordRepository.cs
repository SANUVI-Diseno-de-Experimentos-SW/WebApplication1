using WebApplication1.patient_management.Domain.Entities;

namespace WebApplication1.patient_management.Domain.Repositories;



/// <summary>
/// Repositorio de historias clínicas
/// </summary>
public interface IMedicalRecordRepository
{
    /// <summary>
    /// Guarda una nueva historia clínica
    /// </summary>
    Task<MedicalRecord> SaveAsync(MedicalRecord medicalRecord);

    /// <summary>
    /// Busca una historia clínica por ID
    /// </summary>
    Task<MedicalRecord?> FindByIdAsync(string medicalRecordId);

    /// <summary>
    /// Busca una historia clínica por ID de paciente
    /// </summary>
    Task<MedicalRecord?> FindByPatientIdAsync(string patientId);

    /// <summary>
    /// Actualiza una historia clínica existente
    /// </summary>
    Task UpdateAsync(MedicalRecord medicalRecord);

    /// <summary>
    /// Elimina una historia clínica
    /// </summary>
    Task DeleteAsync(string medicalRecordId);
}