using MongoDB.Driver;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Infrastructure.Mappers;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;

public class MongoNurseAssignmentRepository : INurseAssignmentRepository
{
    private readonly IMongoCollection<NurseAssignmentDocument> _collection;
    private readonly ILogger<MongoNurseAssignmentRepository> _logger;

    public MongoNurseAssignmentRepository(IMongoDatabase database, ILogger<MongoNurseAssignmentRepository> logger)
    {
        _collection = database.GetCollection<NurseAssignmentDocument>("nurseassignments");
        _logger = logger;
    }

    public async Task<NurseAssignment> SaveAsync(NurseAssignment assignment)
    {
        try
        {
            // ✅ Usar el mapper correctamente
            var document = NurseAssignmentMapper.ToPersistence(assignment);
            
            _logger.LogInformation("📝 Guardando asignación: NurseId={NurseId}, FacilityId={FacilityId}", 
                document.NurseId, document.FacilityId);

            await _collection.InsertOneAsync(document);
            _logger.LogInformation("✅ Asignación de enfermero creada: {NurseAssignmentId}", document.NurseAssignmentId);

            return assignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al guardar asignación de enfermero");
            throw;
        }
    }

    public async Task<List<NurseAssignment>> FindByFacilityIdAsync(string facilityId)
    {
        var filter = Builders<NurseAssignmentDocument>.Filter.Eq(x => x.FacilityId, facilityId);
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(NurseAssignmentMapper.ToDomain).ToList();
    }

    public async Task<NurseAssignment?> FindByNurseIdAsync(string nurseId)
    {
        var filter = Builders<NurseAssignmentDocument>.Filter.Eq(x => x.NurseId, nurseId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return NurseAssignmentMapper.ToDomain(document);
    }

    public async Task<NurseAssignment?> FindActiveByFacilityIdAsync(string facilityId)
    {
        var filter = Builders<NurseAssignmentDocument>.Filter.Eq(x => x.FacilityId, facilityId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return NurseAssignmentMapper.ToDomain(document);
    }

    public async Task<NurseAssignment?> FindActiveByNurseIdAsync(string nurseId)
    {
        var filter = Builders<NurseAssignmentDocument>.Filter.Eq(x => x.NurseId, nurseId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return NurseAssignmentMapper.ToDomain(document);
    }
}