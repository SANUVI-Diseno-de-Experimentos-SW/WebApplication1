using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Mapper;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;


public class MongoMedicalRecordRepository : IMedicalRecordRepository
{
    private readonly IMongoCollection<MedicalRecordDocument> _collection;
    private readonly ILogger<MongoMedicalRecordRepository> _logger;

    public MongoMedicalRecordRepository(IMongoDatabase database, ILogger<MongoMedicalRecordRepository> logger)
    {
        _collection = database.GetCollection<MedicalRecordDocument>("medicalrecords");
        _logger = logger;
    }

    public async Task<MedicalRecord> SaveAsync(MedicalRecord medicalRecord)
    {
        var document = MedicalRecordMapper.ToPersistence(medicalRecord);
        await _collection.InsertOneAsync(document);
        return medicalRecord;
    }
    public async Task<MedicalRecord?> FindByIdAsync(string medicalRecordId)
    {
        var filter = Builders<MedicalRecordDocument>.Filter.Eq(x => x.MedicalRecordId, medicalRecordId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return MedicalRecordMapper.ToDomain(document); // ✅ Usa el mapper actualizado
    }

    public async Task<MedicalRecord?> FindByPatientIdAsync(string patientId)
    {
        var filter = Builders<MedicalRecordDocument>.Filter.Eq(x => x.PatientId, patientId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return MedicalRecordMapper.ToDomain(document); // ✅ Usa el mapper actualizado
    }

    // MongoMedicalRecordRepository.cs

    // MongoMedicalRecordRepository.cs - Método UpdateAsync

public async Task UpdateAsync(MedicalRecord medicalRecord)
{
    try
    {
        Console.WriteLine($"🔍 UpdateAsync - MedicalRecordId: {medicalRecord?.Id}");
        
        if (medicalRecord == null)
            throw new ArgumentNullException(nameof(medicalRecord));

        var data = medicalRecord.ToPrimitives();
        var id = data.Id;

        Console.WriteLine($"🔍 ID para actualizar: {id}");

        var filter = Builders<MedicalRecordDocument>.Filter.Eq(x => x.MedicalRecordId, id);
        
        // Buscar el documento existente
        var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existingDoc == null)
        {
            Console.WriteLine($"❌ No se encontró medical record con ID: {id}");
            throw new Exception($"Medical record with ID {id} not found");
        }

        // Convertir controles
        var controls = data.Controls.Select(c => new ControlDocument
        {
            Id = c.Id,
            Date = c.Date,
            HemoglobinLevel = c.HemoglobinLevel,
            AnemiaStatus = c.AnemiaStatus
        }).ToList();

        Console.WriteLine($"🔍 Controls a actualizar: {controls.Count}");

        var update = Builders<MedicalRecordDocument>.Update
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.Weight, data.Weight)
            .Set(x => x.Height, data.Height)
            .Set(x => x.Gender, data.Gender)
            .Set(x => x.MotivoConsulta, data.MotivoConsulta)
            .Set(x => x.Observaciones, data.Observaciones)
            .Set(x => x.HemoglobinLevel, data.HemoglobinLevel)
            .Set(x => x.Controls, controls);

        Console.WriteLine($"🔍 Aplicando update...");
        var result = await _collection.UpdateOneAsync(filter, update);
        
        Console.WriteLine($"✅ Update completado. MatchedCount: {result.MatchedCount}, ModifiedCount: {result.ModifiedCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en UpdateAsync: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        throw;
    }
}
    public async Task DeleteAsync(string medicalRecordId)
    {
        var filter = Builders<MedicalRecordDocument>.Filter.Eq(x => x.MedicalRecordId, medicalRecordId);
        await _collection.DeleteOneAsync(filter);
    }
}