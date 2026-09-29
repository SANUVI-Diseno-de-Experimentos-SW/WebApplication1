
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

namespace WebApplication1.HealthyFacility.Domain.Models.Aggregate;



public class HealthFacility
{
    public string Id { get; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string DistrictId { get; private set; }
    public string DistrictName { get; private set; }
    public Coordinates Coordinates { get; private set; }
    public string PhoneNumber { get; private set; }
    public List<string> Services { get; private set; }
    public OperatingSchedule OperatingSchedule { get; private set; }
    public string ScheduleOfOperation { get; private set; }
    public FacilityStatus Status { get; private set; }
    public List<NurseAssignment> NurseAssignments { get; private set; }

    public HealthFacility(
        string id,
        string name,
        string address,
        string districtId,
        string districtName,
        Coordinates coordinates,
        string phoneNumber,
        List<string> services,
        OperatingSchedule operatingSchedule,
        string scheduleOfOperation,
        FacilityStatus status,
        List<NurseAssignment> nurseAssignments)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Health facility id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Health facility name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required", nameof(address));

        if (string.IsNullOrWhiteSpace(districtId))
            throw new ArgumentException("District id is required", nameof(districtId));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        Id = id;
        Name = name;
        Address = address;
        DistrictId = districtId;
        DistrictName = districtName;
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
        PhoneNumber = phoneNumber;
        Services = services ?? new List<string>();
        OperatingSchedule = operatingSchedule ?? throw new ArgumentNullException(nameof(operatingSchedule));
        ScheduleOfOperation = scheduleOfOperation;
        Status = status;
        NurseAssignments = nurseAssignments ?? new List<NurseAssignment>();
    }

    // Constructor privado para serialización
    private HealthFacility() { }

    public void Activate()
    {
        Status = FacilityStatus.ACTIVE;
    }

    public void Deactivate()
    {
        Status = FacilityStatus.INACTIVE;
    }

    public void UpdateServices(List<string> services)
    {
        Services = services ?? new List<string>();
    }

    public void AssignNurse(NurseAssignment assignment)
    {
        var alreadyAssigned = NurseAssignments.Any(n => n.NurseId == assignment.NurseId);

        if (alreadyAssigned)
            throw new InvalidOperationException("Nurse already assigned");

        NurseAssignments.Add(assignment);
    }

    public bool IsActive() => Status == FacilityStatus.ACTIVE;
    public bool IsInactive() => Status == FacilityStatus.INACTIVE;

    public HealthFacilityPrimitives ToPrimitives()
    {
        return new HealthFacilityPrimitives
        {
            Id = Id,
            Name = Name,
            Address = Address,
            DistrictId = DistrictId,
            DistrictName = DistrictName,
            Coordinates = Coordinates.ToPrimitives(),
            PhoneNumber = PhoneNumber,
            Services = Services,
            OperatingSchedule = OperatingSchedule.ToPrimitives(),
            ScheduleOfOperation = ScheduleOfOperation,
            Status = Status.ToStringValue(),
            NurseAssignments = NurseAssignments.Select(n => n.ToPrimitives()).ToList()
        };
    }

    public class HealthFacilityPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DistrictId { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public Coordinates.CoordinatesPrimitives Coordinates { get; set; } = new();
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public OperatingSchedule.OperatingSchedulePrimitives OperatingSchedule { get; set; } = new();
        public string ScheduleOfOperation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<NurseAssignment.NurseAssignmentPrimitives> NurseAssignments { get; set; } = new();
    }
}