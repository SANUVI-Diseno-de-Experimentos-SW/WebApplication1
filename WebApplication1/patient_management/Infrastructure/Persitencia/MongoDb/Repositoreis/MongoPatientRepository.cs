using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Mapper;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;


public class MongoPatientRepository : IPatientRepository
{
    private readonly IMongoCollection<PatientDocument> _collection;
    private readonly ILogger<MongoPatientRepository> _logger;

    public MongoPatientRepository(IMongoDatabase database, ILogger<MongoPatientRepository> logger)
    {
        _collection = database.GetCollection<PatientDocument>("patients");
        _logger = logger;
    }

    public async Task<Patient> SaveAsync(Patient patient)
    {
        // ✅ Usar ToPersistence que ahora retorna PatientDocument
        var document = PatientMapper.ToPersistence(patient);
        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Paciente creado: {PatientId}", document.PatientId);
        return patient;
    }

    public async Task<Patient?> FindByIdAsync(string patientId)
    {
        var filter = Builders<PatientDocument>.Filter.Eq(x => x.PatientId, patientId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return PatientMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Patient>> FindByMotherIdAsync(string motherId)
    {
        var filter = Builders<PatientDocument>.Filter.Eq(x => x.MotherId, motherId);
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(PatientMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Patient>> FindByNurseIdAsync(string nurseId)
    {
        var filter = Builders<PatientDocument>.Filter.Eq(x => x.NurseId, nurseId);
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(PatientMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Patient>> FindByStatusAsync(PatientStatus status)
    {
        var filter = Builders<PatientDocument>.Filter.Eq(x => x.Status, status.ToStringValue());
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(PatientMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Patient>> FindAllAsync()
    {
        var documents = await _collection.Find(_ => true).ToListAsync();
        return documents.Select(PatientMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Patient>> FindPatientsAssignedToNurseAsync(string nurseId, string? searchTerm = null)
    {
        var filter = Builders<PatientDocument>.Filter.Eq(x => x.NurseId, nurseId);
        var documents = await _collection.Find(filter).ToListAsync();

        var patients = documents.Select(PatientMapper.ToDomain).ToList();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            patients = patients.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term)
            ).ToList();
        }

        return patients.AsReadOnly();
    }

    public async Task<IReadOnlyList<Patient>> FindPatientsEligibleForDischargeAsync(string nurseId, string? searchTerm = null)
    {
        var filter = Builders<PatientDocument>.Filter.And(
            Builders<PatientDocument>.Filter.Eq(x => x.NurseId, nurseId),
            Builders<PatientDocument>.Filter.Ne(x => x.Status, PatientStatus.Discharged.ToStringValue())
        );

        var documents = await _collection.Find(filter).ToListAsync();
        var patients = documents.Select(PatientMapper.ToDomain).ToList();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            patients = patients.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term)
            ).ToList();
        }

        return patients.AsReadOnly();
    }

    public async Task<int> CountActivePatientsByNurseAsync(string nurseId)
    {
        var filter = Builders<PatientDocument>.Filter.And(
            Builders<PatientDocument>.Filter.Eq(x => x.NurseId, nurseId),
            Builders<PatientDocument>.Filter.Ne(x => x.Status, PatientStatus.Discharged.ToStringValue())
        );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        try
        {
            var data = patient.ToPrimitives();
            var patientId = data.Id;

            // ✅ LOG para depuración
            _logger.LogInformation("🔍 UpdateAsync - patientId: {PatientId}", patientId);
            _logger.LogInformation("🔍 UpdateAsync - NurseId: {NurseId}", data.NurseId ?? "NULL");
            _logger.LogInformation("🔍 UpdateAsync - FacilityId: {FacilityId}", data.FacilityId ?? "NULL");
            _logger.LogInformation("🔍 UpdateAsync - Status: {Status}", data.Status);

            // ✅ Obtener el documento existente para preservar el _id
            var filter = Builders<PatientDocument>.Filter.Eq(x => x.PatientId, patientId);
            var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync();

            if (existingDocument == null)
            {
                _logger.LogWarning("No se encontró paciente para actualizar: {PatientId}", patientId);
                throw new Exception($"Patient with id {patientId} not found");
            }

            // ✅ CORREGIDO: Usar UpdateOneAsync con Set en lugar de ReplaceOneAsync
            var update = Builders<PatientDocument>.Update
                .Set(x => x.Name, data.Name)
                .Set(x => x.LastName, data.LastName)
                .Set(x => x.BirthDate, data.BirthDate)
                .Set(x => x.CurrentWeight, data.CurrentWeight)
                .Set(x => x.CurrentHeight, data.CurrentHeight)
                .Set(x => x.MotherId, data.MotherId)
                .Set(x => x.NurseId, data.NurseId)
                .Set(x => x.Gender, data.Gender)
                .Set(x => x.FacilityId, data.FacilityId)
                .Set(x => x.Status, data.Status)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0 && result.MatchedCount == 0)
            {
                _logger.LogWarning("No se encontró paciente para actualizar: {PatientId}", patientId);
                throw new Exception($"Patient with id {patientId} not found");
            }

            _logger.LogInformation("Paciente actualizado: {PatientId}", patientId);
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar paciente");
            throw;
        }
    }
    
    
    public async Task<MedicalRecord?> FindMedicalRecordByPatientIdAsync(string patientId)
    {
        // Esto debería usar el repositorio de MedicalRecord
        // Por ahora retornamos null
        return null;
    }

    public async Task<MedicalRecord> SaveMedicalRecordAsync(MedicalRecord medicalRecord)
    {
        // Esto debería usar el repositorio de MedicalRecord
        return medicalRecord;
    }

    public async Task<bool> HasMedicalRecordAsync(string patientId)
    {
        // Esto debería usar el repositorio de MedicalRecord
        return false;
    }

    public async Task<Control> SaveControlAsync(Control control)
    {
        // Esto debería usar el repositorio de MedicalRecord
        return control;
    }

    public async Task<IReadOnlyList<Control>> FindControlsByMedicalRecordIdAsync(string medicalRecordId)
    {
        // Esto debería usar el repositorio de MedicalRecord
        return new List<Control>().AsReadOnly();
    }
}