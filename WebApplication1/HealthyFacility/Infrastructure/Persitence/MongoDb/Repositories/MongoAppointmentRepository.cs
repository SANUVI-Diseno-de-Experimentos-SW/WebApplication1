using MongoDB.Driver;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Infrastructure.Mappers;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;


public class MongoAppointmentRepository : IAppointmentRepository
{
    private readonly IMongoCollection<AppointmentDocument> _collection;
    private readonly ILogger<MongoAppointmentRepository> _logger;

    public MongoAppointmentRepository(IMongoDatabase database, ILogger<MongoAppointmentRepository> logger)
    {
        _collection = database.GetCollection<AppointmentDocument>("appointments");
        _logger = logger;
    }

    public async Task<Appointment> SaveAsync(Appointment appointment)
    {
        var data = AppointmentMapper.ToPersistence(appointment);

        var document = new AppointmentDocument
        {
            AppointmentId = (string)data.GetType().GetProperty("id")?.GetValue(data, null)!,
            FacilityId = (string)data.GetType().GetProperty("facilityId")?.GetValue(data, null)!,
            PatientId = (string)data.GetType().GetProperty("patientId")?.GetValue(data, null)!,
            MotherId = (string)data.GetType().GetProperty("motherId")?.GetValue(data, null)!,
            NurseId = (string?)data.GetType().GetProperty("nurseId")?.GetValue(data, null),
            AppointmentDate = (string)data.GetType().GetProperty("appointmentDate")?.GetValue(data, null)!,
            AppointmentTime = (string)data.GetType().GetProperty("appointmentTime")?.GetValue(data, null)!,
            Status = (string)data.GetType().GetProperty("status")?.GetValue(data, null)!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Cita creada: {AppointmentId}", document.AppointmentId);

        return appointment;
    }

    public async Task<Appointment?> FindByIdAsync(string id)
    {
        var filter = Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentId, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return AppointmentMapper.ToDomain(document);
    }

    public async Task<List<Appointment>> FindByPatientIdAsync(string patientId)
    {
        var filter = Builders<AppointmentDocument>.Filter.Eq(x => x.PatientId, patientId);
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(AppointmentMapper.ToDomain).ToList();
    }

    public async Task<Appointment?> FindByFacilityAndDateTimeAsync(string facilityId, string appointmentDate, string appointmentTime)
    {
        var filter = Builders<AppointmentDocument>.Filter.And(
            Builders<AppointmentDocument>.Filter.Eq(x => x.FacilityId, facilityId),
            Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentDate, appointmentDate),
            Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentTime, appointmentTime),
            Builders<AppointmentDocument>.Filter.Eq(x => x.Status, AppointmentStatus.CONFIRMED.ToStringValue())
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return AppointmentMapper.ToDomain(document);
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        var data = AppointmentMapper.ToPersistence(appointment);
        var appointmentId = (string)data.GetType().GetProperty("id")?.GetValue(data, null)!;

        var filter = Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentId, appointmentId);

        var update = Builders<AppointmentDocument>.Update
            .Set(x => x.NurseId, (string?)data.GetType().GetProperty("nurseId")?.GetValue(data, null))
            .Set(x => x.Status, (string)data.GetType().GetProperty("status")?.GetValue(data, null)!)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Cita actualizada: {AppointmentId}", appointmentId);
    }

    public async Task<List<Appointment>> FindConfirmedByNurseIdAsync(string nurseId)
    {
        var filter = Builders<AppointmentDocument>.Filter.And(
            Builders<AppointmentDocument>.Filter.Eq(x => x.NurseId, nurseId),
            Builders<AppointmentDocument>.Filter.Eq(x => x.Status, AppointmentStatus.CONFIRMED.ToStringValue())
        );

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(AppointmentMapper.ToDomain).ToList();
    }

    public async Task<List<Appointment>> FindByFacilityAndDateAsync(string facilityId, string appointmentDate)
    {
        var filter = Builders<AppointmentDocument>.Filter.And(
            Builders<AppointmentDocument>.Filter.Eq(x => x.FacilityId, facilityId),
            Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentDate, appointmentDate),
            Builders<AppointmentDocument>.Filter.Eq(x => x.Status, AppointmentStatus.CONFIRMED.ToStringValue())
        );

        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(AppointmentMapper.ToDomain).ToList();
    }

    public async Task<Appointment?> FindNextAppointmentByMotherIdAsync(string motherId)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var currentTime = DateTime.UtcNow.ToString("HH:mm");

        var filter = Builders<AppointmentDocument>.Filter.And(
            Builders<AppointmentDocument>.Filter.Eq(x => x.MotherId, motherId),
            Builders<AppointmentDocument>.Filter.Eq(x => x.Status, AppointmentStatus.CONFIRMED.ToStringValue()),
            Builders<AppointmentDocument>.Filter.Or(
                Builders<AppointmentDocument>.Filter.Gt(x => x.AppointmentDate, today),
                Builders<AppointmentDocument>.Filter.And(
                    Builders<AppointmentDocument>.Filter.Eq(x => x.AppointmentDate, today),
                    Builders<AppointmentDocument>.Filter.Gt(x => x.AppointmentTime, currentTime)
                )
            )
        );

        var document = await _collection
            .Find(filter)
            .SortBy(x => x.AppointmentDate)
            .ThenBy(x => x.AppointmentTime)
            .FirstOrDefaultAsync();

        if (document == null) return null;

        return AppointmentMapper.ToDomain(document);
    }
}