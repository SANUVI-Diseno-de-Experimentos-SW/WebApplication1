using MongoDB.Driver;
using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Infrastructure.Mappers;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;

public class MongoHealthFacilityRepository : IHealthFacilityRepository
{
    private readonly IMongoCollection<HealthFacilityDocument> _collection;
    private readonly ILogger<MongoHealthFacilityRepository> _logger;

    public MongoHealthFacilityRepository(IMongoDatabase database, ILogger<MongoHealthFacilityRepository> logger)
    {
        _collection = database.GetCollection<HealthFacilityDocument>("healthfacilities");
        _logger = logger;
    }

    public async Task<HealthFacility> SaveAsync(HealthFacility facility)
    {
        try
        {
            _logger.LogInformation("📝 Guardando posta: {FacilityName}", facility.Name);

            var data = HealthFacilityMapper.ToPersistence(facility);

            // ✅ Crear documentos de NurseAssignment
            var nurseAssignments = data.NurseAssignments?
                .Select(na => new NurseAssignmentDocument
                {
                    NurseAssignmentId = na.Id,      // ✅ Usar NurseAssignmentId
                    FacilityId = na.FacilityId,
                    NurseId = na.NurseId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList() ?? new List<NurseAssignmentDocument>();

            _logger.LogInformation("📋 NurseAssignments a guardar: {Count}", nurseAssignments.Count);

            var document = new HealthFacilityDocument
            {
                HealthFacilityId = data.Id,
                Name = data.Name,
                Address = data.Address,
                DistrictId = data.DistrictId,
                DistrictName = data.DistrictName,
                Coordinates = new CoordinatesDocument
                {
                    Lat = data.Latitude,
                    Lng = data.Longitude
                },
                PhoneNumber = data.PhoneNumber,
                Services = data.Services ?? new List<string>(),
                OperatingSchedule = new OperatingScheduleDocument
                {
                    AvailableDays = data.AvailableDays ?? new List<string>(),
                    AvailableSlots = data.AvailableSlots ?? new List<string>()
                },
                ScheduleOfOperation = data.ScheduleOfOperation,
                Status = data.Status,
                NurseAssignments = nurseAssignments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(document);
            _logger.LogInformation("✅ Posta de salud creada: {HealthFacilityId}", document.HealthFacilityId);

            return facility;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al guardar posta: {FacilityName}", facility.Name);
            throw;
        }
    }
    
    // ✅ CORREGIDO: Pasar HealthFacilityDocument directamente
    public async Task<HealthFacility?> FindByIdAsync(string id)
    {
        var filter = Builders<HealthFacilityDocument>.Filter.Eq(x => x.HealthFacilityId, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return HealthFacilityMapper.ToDomain(document);
    }

    // ✅ CORREGIDO: Pasar HealthFacilityDocument directamente
    public async Task<List<HealthFacility>> FindAllAsync()
    {
        var documents = await _collection.Find(_ => true).ToListAsync();
        return documents.Select(HealthFacilityMapper.ToDomain).ToList();
    }

    // ✅ CORREGIDO: Pasar HealthFacilityDocument directamente
    public async Task<List<HealthFacility>> FindActiveFacilitiesAsync()
    {
        var filter = Builders<HealthFacilityDocument>.Filter.Eq(x => x.Status, FacilityStatus.ACTIVE.ToStringValue());
        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Select(HealthFacilityMapper.ToDomain).ToList();
    }

    public async Task UpdateAsync(HealthFacility facility)
    {
        var data = HealthFacilityMapper.ToPersistence(facility);
        var filter = Builders<HealthFacilityDocument>.Filter.Eq(x => x.HealthFacilityId, data.Id);

        var update = Builders<HealthFacilityDocument>.Update
            .Set(x => x.Name, data.Name)
            .Set(x => x.Address, data.Address)
            .Set(x => x.DistrictId, data.DistrictId)
            .Set(x => x.DistrictName, data.DistrictName)
            .Set(x => x.Coordinates, new CoordinatesDocument { Lat = data.Latitude, Lng = data.Longitude })
            .Set(x => x.PhoneNumber, data.PhoneNumber)
            .Set(x => x.Services, data.Services ?? new List<string>())
            .Set(x => x.OperatingSchedule, new OperatingScheduleDocument
            {
                AvailableDays = data.AvailableDays ?? new List<string>(),
                AvailableSlots = data.AvailableSlots ?? new List<string>()
            })
            .Set(x => x.ScheduleOfOperation, data.ScheduleOfOperation)
            .Set(x => x.Status, data.Status)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Posta de salud actualizada: {HealthFacilityId}", data.Id);
    }
}