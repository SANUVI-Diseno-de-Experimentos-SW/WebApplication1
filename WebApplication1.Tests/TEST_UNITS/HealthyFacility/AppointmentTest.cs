using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using Xunit;

namespace WebApplication1.Tests.HealthyFacility;

public class AppointmentTests
{
    // ==========================================
    // HELPERS
    // ==========================================

    private static Appointment CreateAppointment(
        string id = "appointment-001",
        string facilityId = "facility-001",
        string patientId = "patient-001",
        string motherId = "mother-001",
        string appointmentDate = "2026-11-05",
        string appointmentTime = "09:30",
        string? nurseId = null,
        AppointmentStatus status = AppointmentStatus.CONFIRMED)
    {
        return new Appointment(
            id,
            facilityId,
            patientId,
            motherId,
            appointmentDate,
            appointmentTime,
            nurseId,
            status);
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesAppointmentSuccessfully()
    {
        var appointment = new Appointment(
            "appointment-001", "facility-001", "patient-001", "mother-001",
            "2026-11-05", "09:30", "nurse-001", AppointmentStatus.CANCELLED);

        Assert.NotNull(appointment);
        Assert.Equal("appointment-001", appointment.Id);
        Assert.Equal("facility-001", appointment.FacilityId);
        Assert.Equal("patient-001", appointment.PatientId);
        Assert.Equal("mother-001", appointment.MotherId);
        Assert.Equal("2026-11-05", appointment.AppointmentDate);
        Assert.Equal("09:30", appointment.AppointmentTime);
        Assert.Equal("nurse-001", appointment.NurseId);
        Assert.Equal(AppointmentStatus.CANCELLED, appointment.Status);
    }

    [Fact]
    public void Constructor_WithoutOptionalData_UsesNullNurseAndConfirmedStatus()
    {
        var appointment = new Appointment(
            "appointment-001", "facility-001", "patient-001", "mother-001",
            "2026-11-05", "09:30");

        Assert.Null(appointment.NurseId);
        Assert.Equal(AppointmentStatus.CONFIRMED, appointment.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(id: invalid!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFacilityId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(facilityId: invalid!));

        Assert.Equal("facilityId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPatientId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(patientId: invalid!));

        Assert.Equal("patientId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMotherId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(motherId: invalid!));

        Assert.Equal("motherId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAppointmentDate_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(appointmentDate: invalid!));

        Assert.Equal("appointmentDate", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAppointmentTime_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateAppointment(appointmentTime: invalid!));

        Assert.Equal("appointmentTime", ex.ParamName);
    }

    // ==========================================
    // AssignNurse
    // ==========================================

    [Fact]
    public void AssignNurse_WithValidNurseId_SetsNurse()
    {
        var appointment = CreateAppointment();

        appointment.AssignNurse("nurse-001");

        Assert.Equal("nurse-001", appointment.NurseId);
    }

    [Fact]
    public void AssignNurse_WhenAlreadyHasNurse_ReplacesIt()
    {
        var appointment = CreateAppointment(nurseId: "nurse-001");

        appointment.AssignNurse("nurse-002");

        Assert.Equal("nurse-002", appointment.NurseId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AssignNurse_WithInvalidNurseId_ThrowsArgumentException(string? invalid)
    {
        var appointment = CreateAppointment();

        var ex = Assert.ThrowsAny<ArgumentException>(() => appointment.AssignNurse(invalid!));

        Assert.Equal("nurseId", ex.ParamName);
    }

    [Fact]
    public void AssignNurse_WithInvalidNurseId_DoesNotChangeExistingNurse()
    {
        var appointment = CreateAppointment(nurseId: "nurse-001");

        Assert.ThrowsAny<ArgumentException>(() => appointment.AssignNurse("  "));

        Assert.Equal("nurse-001", appointment.NurseId);
    }

    // ==========================================
    // Cancel
    // ==========================================

    [Fact]
    public void Cancel_WhenConfirmed_SetsStatusToCancelled()
    {
        var appointment = CreateAppointment(status: AppointmentStatus.CONFIRMED);

        appointment.Cancel();

        Assert.Equal(AppointmentStatus.CANCELLED, appointment.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsInvalidOperationException()
    {
        var appointment = CreateAppointment();
        appointment.Cancel();

        Assert.Throws<InvalidOperationException>(() => appointment.Cancel());
    }

    [Fact]
    public void Cancel_WhenCreatedAsCancelled_ThrowsInvalidOperationException()
    {
        var appointment = CreateAppointment(status: AppointmentStatus.CANCELLED);

        Assert.Throws<InvalidOperationException>(() => appointment.Cancel());
    }

    [Fact]
    public void Cancel_DoesNotChangeOtherFields()
    {
        var appointment = CreateAppointment(nurseId: "nurse-001");

        appointment.Cancel();

        Assert.Equal("appointment-001", appointment.Id);
        Assert.Equal("nurse-001", appointment.NurseId);
        Assert.Equal("2026-11-05", appointment.AppointmentDate);
        Assert.Equal("09:30", appointment.AppointmentTime);
    }

    // ==========================================
    // Confirm
    // ==========================================

    [Fact]
    public void Confirm_WhenCancelled_SetsStatusToConfirmed()
    {
        var appointment = CreateAppointment(status: AppointmentStatus.CANCELLED);

        appointment.Confirm();

        Assert.Equal(AppointmentStatus.CONFIRMED, appointment.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_KeepsItConfirmedWithoutThrowing()
    {
        var appointment = CreateAppointment(status: AppointmentStatus.CONFIRMED);

        var exception = Record.Exception(() => appointment.Confirm());

        Assert.Null(exception);
        Assert.Equal(AppointmentStatus.CONFIRMED, appointment.Status);
    }

    [Fact]
    public void Cancel_AfterConfirmingACancelledAppointment_CancelsItAgain()
    {
        var appointment = CreateAppointment();
        appointment.Cancel();
        appointment.Confirm();

        appointment.Cancel();

        Assert.Equal(AppointmentStatus.CANCELLED, appointment.Status);
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var appointment = CreateAppointment(nurseId: "nurse-001");

        var primitives = appointment.ToPrimitives();

        Assert.Equal(appointment.Id, primitives.Id);
        Assert.Equal(appointment.FacilityId, primitives.FacilityId);
        Assert.Equal(appointment.PatientId, primitives.PatientId);
        Assert.Equal(appointment.MotherId, primitives.MotherId);
        Assert.Equal("nurse-001", primitives.NurseId);
        Assert.Equal(appointment.AppointmentDate, primitives.AppointmentDate);
        Assert.Equal(appointment.AppointmentTime, primitives.AppointmentTime);
        Assert.Equal(appointment.Status.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_WithoutNurse_ReturnsNullNurseId()
    {
        var appointment = CreateAppointment();

        var primitives = appointment.ToPrimitives();

        Assert.Null(primitives.NurseId);
    }

    [Fact]
    public void ToPrimitives_AfterCancel_ReflectsCancelledStatus()
    {
        var appointment = CreateAppointment();
        appointment.Cancel();

        var primitives = appointment.ToPrimitives();

        Assert.Equal(AppointmentStatus.CANCELLED.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_AfterAssignNurse_ReflectsNurse()
    {
        var appointment = CreateAppointment();
        appointment.AssignNurse("nurse-009");

        var primitives = appointment.ToPrimitives();

        Assert.Equal("nurse-009", primitives.NurseId);
    }
}
