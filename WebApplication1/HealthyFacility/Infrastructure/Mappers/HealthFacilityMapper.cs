using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

namespace WebApplication1.HealthyFacility.Infrastructure.Mappers;

public static class HealthFacilityMapper
{
    public static HealthFacility ToDomain(HealthFacilityDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var nurseAssignments = document.NurseAssignments?
            .Select(na => new NurseAssignment(
                na.NurseAssignmentId,
                na.FacilityId,
                na.NurseId
            ))
            .ToList() ?? new List<NurseAssignment>();

        return new HealthFacility(
            document.HealthFacilityId,
            document.Name,
            document.Address,
            document.DistrictId,
            document.DistrictName,
            new Coordinates(
                document.Coordinates.Lat,
                document.Coordinates.Lng
            ),
            document.PhoneNumber,
            document.Services ?? new List<string>(),
            new OperatingSchedule(
                document.OperatingSchedule?.AvailableDays ?? new List<string>(),
                document.OperatingSchedule?.AvailableSlots ?? new List<string>()
            ),
            document.ScheduleOfOperation,
            FacilityStatusExtensions.FromString(document.Status),
            nurseAssignments
        );
    }

    public static HealthFacilityPersistenceDto ToPersistence(HealthFacility facility)
    {
        if (facility == null)
            throw new ArgumentNullException(nameof(facility));

        var data = facility.ToPrimitives();

        return new HealthFacilityPersistenceDto
        {
            Id = data.Id ?? string.Empty,
            Name = data.Name ?? string.Empty,
            Address = data.Address ?? string.Empty,
            DistrictId = data.DistrictId ?? string.Empty,
            DistrictName = data.DistrictName ?? string.Empty,
            Latitude = data.Coordinates?.Lat ?? 0,
            Longitude = data.Coordinates?.Lng ?? 0,
            PhoneNumber = data.PhoneNumber ?? string.Empty,
            Services = data.Services ?? new List<string>(),
            AvailableDays = data.OperatingSchedule?.AvailableDays ?? new List<string>(),
            AvailableSlots = data.OperatingSchedule?.AvailableSlots ?? new List<string>(),
            ScheduleOfOperation = data.ScheduleOfOperation ?? string.Empty,
            Status = data.Status ?? FacilityStatus.ACTIVE.ToStringValue(),
            NurseAssignments = data.NurseAssignments ?? new List<NurseAssignment.NurseAssignmentPrimitives>()
        };
    }

    public class HealthFacilityPersistenceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DistrictId { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public List<string> AvailableDays { get; set; } = new();
        public List<string> AvailableSlots { get; set; } = new();
        public string ScheduleOfOperation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<NurseAssignment.NurseAssignmentPrimitives> NurseAssignments { get; set; } = new();
    }
}