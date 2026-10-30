using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using Xunit;
using static WebApplication1.Tests.HealthyFacility.HealthFacilityTestData;

namespace WebApplication1.Tests.HealthyFacility;

public class HealthFacilityTests
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesFacilitySuccessfully()
    {
        var coordinates = NewCoordinates();
        var schedule = NewOperatingSchedule();
        var services = NewServices();
        var assignments = new List<NurseAssignment> { NewNurseAssignment("nurse-001") };

        var facility = new HealthFacility(
            "facility-001", "Centro de Salud Central", "Av. Principal 123",
            "district-001", "San Isidro", coordinates, "014567890",
            services, schedule, "Lunes a viernes", FacilityStatus.ACTIVE, assignments);

        Assert.NotNull(facility);
        Assert.Equal("facility-001", facility.Id);
        Assert.Equal("Centro de Salud Central", facility.Name);
        Assert.Equal("Av. Principal 123", facility.Address);
        Assert.Equal("district-001", facility.DistrictId);
        Assert.Equal("San Isidro", facility.DistrictName);
        Assert.Same(coordinates, facility.Coordinates);
        Assert.Equal("014567890", facility.PhoneNumber);
        Assert.Same(services, facility.Services);
        Assert.Same(schedule, facility.OperatingSchedule);
        Assert.Equal("Lunes a viernes", facility.ScheduleOfOperation);
        Assert.Equal(FacilityStatus.ACTIVE, facility.Status);
        Assert.Same(assignments, facility.NurseAssignments);
    }

    [Fact]
    public void Constructor_WithNullServices_UsesEmptyList()
    {
        var facility = new HealthFacility(
            "facility-001", "Centro de Salud Central", "Av. Principal 123",
            "district-001", "San Isidro", NewCoordinates(), "014567890",
            null!, NewOperatingSchedule(), "Lunes a viernes", FacilityStatus.ACTIVE,
            new List<NurseAssignment>());

        Assert.NotNull(facility.Services);
        Assert.Empty(facility.Services);
    }

    [Fact]
    public void Constructor_WithNullNurseAssignments_UsesEmptyList()
    {
        var facility = new HealthFacility(
            "facility-001", "Centro de Salud Central", "Av. Principal 123",
            "district-001", "San Isidro", NewCoordinates(), "014567890",
            NewServices(), NewOperatingSchedule(), "Lunes a viernes", FacilityStatus.ACTIVE,
            null!);

        Assert.NotNull(facility.NurseAssignments);
        Assert.Empty(facility.NurseAssignments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewFacility(id: invalid!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewFacility(name: invalid!));

        Assert.Equal("name", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAddress_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewFacility(address: invalid!));

        Assert.Equal("address", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidDistrictId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewFacility(districtId: invalid!));

        Assert.Equal("districtId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPhoneNumber_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewFacility(phoneNumber: invalid!));

        Assert.Equal("phoneNumber", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCoordinates_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new HealthFacility(
            "facility-001", "Centro de Salud Central", "Av. Principal 123",
            "district-001", "San Isidro", null!, "014567890",
            NewServices(), NewOperatingSchedule(), "Lunes a viernes", FacilityStatus.ACTIVE,
            new List<NurseAssignment>()));

        Assert.Equal("coordinates", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullOperatingSchedule_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new HealthFacility(
            "facility-001", "Centro de Salud Central", "Av. Principal 123",
            "district-001", "San Isidro", NewCoordinates(), "014567890",
            NewServices(), null!, "Lunes a viernes", FacilityStatus.ACTIVE,
            new List<NurseAssignment>()));

        Assert.Equal("operatingSchedule", ex.ParamName);
    }

    // ==========================================
    // Activate / Deactivate
    // ==========================================

    [Fact]
    public void Activate_WhenInactive_SetsStatusToActive()
    {
        var facility = NewFacility(status: FacilityStatus.INACTIVE);

        facility.Activate();

        Assert.Equal(FacilityStatus.ACTIVE, facility.Status);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsStatusToInactive()
    {
        var facility = NewFacility(status: FacilityStatus.ACTIVE);

        facility.Deactivate();

        Assert.Equal(FacilityStatus.INACTIVE, facility.Status);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_KeepsItActive()
    {
        var facility = NewFacility(status: FacilityStatus.ACTIVE);

        facility.Activate();

        Assert.Equal(FacilityStatus.ACTIVE, facility.Status);
    }

    [Theory]
    [InlineData(FacilityStatus.ACTIVE, true, false)]
    [InlineData(FacilityStatus.INACTIVE, false, true)]
    public void StatusQueries_ReturnExpectedValuesForStatus(
        FacilityStatus status, bool isActive, bool isInactive)
    {
        var facility = NewFacility(status: status);

        Assert.Equal(isActive, facility.IsActive());
        Assert.Equal(isInactive, facility.IsInactive());
    }

    // ==========================================
    // UpdateServices
    // ==========================================

    [Fact]
    public void UpdateServices_ReplacesServicesList()
    {
        var facility = NewFacility();
        var newServices = new List<string> { "Nutricion", "Laboratorio" };

        facility.UpdateServices(newServices);

        Assert.Same(newServices, facility.Services);
    }

    [Fact]
    public void UpdateServices_WithNull_SetsEmptyList()
    {
        var facility = NewFacility();

        facility.UpdateServices(null!);

        Assert.NotNull(facility.Services);
        Assert.Empty(facility.Services);
    }

    [Fact]
    public void UpdateServices_WithEmptyList_ClearsServices()
    {
        var facility = NewFacility();

        facility.UpdateServices(new List<string>());

        Assert.Empty(facility.Services);
    }

    // ==========================================
    // AssignNurse
    // ==========================================

    [Fact]
    public void AssignNurse_WithNewNurse_AddsAssignment()
    {
        var facility = NewFacility();
        var assignment = NewNurseAssignment("nurse-001");

        facility.AssignNurse(assignment);

        Assert.Single(facility.NurseAssignments);
        Assert.Same(assignment, facility.NurseAssignments[0]);
    }

    [Fact]
    public void AssignNurse_WithDifferentNurses_AddsEveryAssignment()
    {
        var facility = NewFacility();

        facility.AssignNurse(NewNurseAssignment("nurse-001"));
        facility.AssignNurse(NewNurseAssignment("nurse-002"));

        Assert.Equal(2, facility.NurseAssignments.Count);
    }

    [Fact]
    public void AssignNurse_WhenNurseAlreadyAssigned_ThrowsInvalidOperationException()
    {
        var facility = NewFacility();
        facility.AssignNurse(NewNurseAssignment("nurse-001"));

        Assert.Throws<InvalidOperationException>(() =>
            facility.AssignNurse(NewNurseAssignment("nurse-001")));
    }

    [Fact]
    public void AssignNurse_WhenNurseAlreadyAssigned_DoesNotAddDuplicate()
    {
        var facility = NewFacility();
        facility.AssignNurse(NewNurseAssignment("nurse-001"));

        Assert.Throws<InvalidOperationException>(() =>
            facility.AssignNurse(NewNurseAssignment("nurse-001")));

        Assert.Single(facility.NurseAssignments);
    }

    [Fact]
    public void AssignNurse_WhenFacilityWasCreatedWithAssignments_DetectsExistingNurse()
    {
        var facility = NewFacility(nurseAssignments: new List<NurseAssignment>
        {
            NewNurseAssignment("nurse-001")
        });

        Assert.Throws<InvalidOperationException>(() =>
            facility.AssignNurse(NewNurseAssignment("nurse-001")));
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var coordinates = NewCoordinates();
        var schedule = NewOperatingSchedule();
        var facility = NewFacility(
            coordinates: coordinates,
            operatingSchedule: schedule,
            status: FacilityStatus.ACTIVE,
            nurseAssignments: new List<NurseAssignment> { NewNurseAssignment("nurse-001") });

        var primitives = facility.ToPrimitives();

        Assert.Equal(facility.Id, primitives.Id);
        Assert.Equal(facility.Name, primitives.Name);
        Assert.Equal(facility.Address, primitives.Address);
        Assert.Equal(facility.DistrictId, primitives.DistrictId);
        Assert.Equal(facility.DistrictName, primitives.DistrictName);
        Assert.Equal(facility.PhoneNumber, primitives.PhoneNumber);
        Assert.Equal(facility.Services, primitives.Services);
        Assert.Equal(facility.ScheduleOfOperation, primitives.ScheduleOfOperation);
        Assert.Equal(facility.Status.ToStringValue(), primitives.Status);
        Assert.Equivalent(coordinates.ToPrimitives(), primitives.Coordinates);
        Assert.Equivalent(schedule.ToPrimitives(), primitives.OperatingSchedule);
        Assert.Single(primitives.NurseAssignments);
    }

    [Fact]
    public void ToPrimitives_WithoutNurseAssignments_ReturnsEmptyList()
    {
        var facility = NewFacility();

        var primitives = facility.ToPrimitives();

        Assert.Empty(primitives.NurseAssignments);
    }

    [Fact]
    public void ToPrimitives_AfterDeactivate_ReflectsInactiveStatus()
    {
        var facility = NewFacility();
        facility.Deactivate();

        var primitives = facility.ToPrimitives();

        Assert.Equal(FacilityStatus.INACTIVE.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_AfterAssignNurse_IncludesAssignment()
    {
        var facility = NewFacility();
        facility.AssignNurse(NewNurseAssignment("nurse-001"));
        facility.AssignNurse(NewNurseAssignment("nurse-002"));

        var primitives = facility.ToPrimitives();

        Assert.Equal(2, primitives.NurseAssignments.Count);
    }
}
