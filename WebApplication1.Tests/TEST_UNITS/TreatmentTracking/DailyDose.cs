using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using Xunit;

namespace WebApplication1.Tests.TreatmentTracking;

public class DailyDoseTests
{
    // ==========================================
    // HELPERS
    // ==========================================

    private static DailyDose CreateDose(
        string id = "dose-001",
        string treatmentId = "treatment-001",
        DateTime? scheduledDate = null,
        DateTime? confirmedAt = null,
        DoseStatus status = DoseStatus.PENDING)
    {
        return new DailyDose(
            id,
            treatmentId,
            scheduledDate ?? DateTime.UtcNow.AddHours(-1),
            confirmedAt,
            status);
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================
    // Nota: DailyDose valida con nameof(Propiedad), por eso el ParamName va con
    // mayuscula inicial ("Id", "TreatmentId", "ScheduledDate").

    [Fact]
    public void Constructor_WithValidData_CreatesDoseSuccessfully()
    {
        var scheduled = DateTime.UtcNow.AddHours(-2);
        var confirmed = DateTime.UtcNow.AddHours(-1);

        var dose = new DailyDose("dose-001", "treatment-001", scheduled, confirmed, DoseStatus.CONFIRMED);

        Assert.NotNull(dose);
        Assert.Equal("dose-001", dose.Id);
        Assert.Equal("treatment-001", dose.TreatmentId);
        Assert.Equal(scheduled, dose.ScheduledDate);
        Assert.Equal(confirmed, dose.ConfirmedAt);
        Assert.Equal(DoseStatus.CONFIRMED, dose.Status);
    }

    [Fact]
    public void Constructor_WithoutConfirmedAt_AllowsNull()
    {
        var dose = CreateDose(confirmedAt: null);

        Assert.Null(dose.ConfirmedAt);
        Assert.Equal(DoseStatus.PENDING, dose.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateDose(id: invalid!));

        Assert.Equal("Id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTreatmentId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateDose(treatmentId: invalid!));

        Assert.Equal("TreatmentId", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultScheduledDate_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateDose(scheduledDate: default(DateTime)));

        Assert.Equal("ScheduledDate", ex.ParamName);
    }

    // ==========================================
    // Confirm
    // ==========================================

    [Fact]
    public void Confirm_WhenPending_SetsConfirmedStatusAndTimestamp()
    {
        var dose = CreateDose();
        var before = DateTime.UtcNow;

        dose.Confirm();

        Assert.Equal(DoseStatus.CONFIRMED, dose.Status);
        Assert.NotNull(dose.ConfirmedAt);
        Assert.InRange(dose.ConfirmedAt!.Value, before, DateTime.UtcNow);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsInvalidOperationException()
    {
        var dose = CreateDose();
        dose.Confirm();

        Assert.Throws<InvalidOperationException>(() => dose.Confirm());
    }

    [Fact]
    public void Confirm_WhenOmitted_ThrowsInvalidOperationException()
    {
        var dose = CreateDose(status: DoseStatus.OMITTED);

        Assert.Throws<InvalidOperationException>(() => dose.Confirm());
    }

    [Fact]
    public void Confirm_WhenOmitted_DoesNotChangeState()
    {
        var dose = CreateDose(status: DoseStatus.OMITTED);

        Assert.Throws<InvalidOperationException>(() => dose.Confirm());

        Assert.Equal(DoseStatus.OMITTED, dose.Status);
        Assert.Null(dose.ConfirmedAt);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_KeepsOriginalConfirmationTime()
    {
        var dose = CreateDose();
        dose.Confirm();
        var firstConfirmation = dose.ConfirmedAt;

        Assert.Throws<InvalidOperationException>(() => dose.Confirm());

        Assert.Equal(firstConfirmation, dose.ConfirmedAt);
    }

    // ==========================================
    // MarkAsOmitted
    // ==========================================

    [Fact]
    public void MarkAsOmitted_WhenPending_SetsOmittedStatus()
    {
        var dose = CreateDose();

        dose.MarkAsOmitted();

        Assert.Equal(DoseStatus.OMITTED, dose.Status);
        Assert.Null(dose.ConfirmedAt);
    }

    [Fact]
    public void MarkAsOmitted_WhenConfirmed_ThrowsInvalidOperationException()
    {
        var dose = CreateDose();
        dose.Confirm();

        Assert.Throws<InvalidOperationException>(() => dose.MarkAsOmitted());
    }

    [Fact]
    public void MarkAsOmitted_WhenConfirmed_DoesNotChangeState()
    {
        var dose = CreateDose();
        dose.Confirm();

        Assert.Throws<InvalidOperationException>(() => dose.MarkAsOmitted());

        Assert.Equal(DoseStatus.CONFIRMED, dose.Status);
        Assert.NotNull(dose.ConfirmedAt);
    }

    [Fact]
    public void MarkAsOmitted_WhenAlreadyOmitted_DoesNothingAndDoesNotThrow()
    {
        var dose = CreateDose(status: DoseStatus.OMITTED);

        var exception = Record.Exception(() => dose.MarkAsOmitted());

        Assert.Null(exception);
        Assert.Equal(DoseStatus.OMITTED, dose.Status);
    }

    // ==========================================
    // CalculateHoursWithoutConfirmation
    // ==========================================
    // Se usan horas con media hora de margen (5.5 h, 3.5 h) para que el redondeo
    // hacia abajo no dependa de los milisegundos que tarda la prueba en ejecutarse.

    [Fact]
    public void CalculateHoursWithoutConfirmation_WhenConfirmed_ReturnsZero()
    {
        var dose = CreateDose(scheduledDate: DateTime.UtcNow.AddHours(-10));
        dose.Confirm();

        Assert.Equal(0, dose.CalculateHoursWithoutConfirmation());
    }

    [Fact]
    public void CalculateHoursWithoutConfirmation_WhenPending_ReturnsWholeHoursSinceSchedule()
    {
        var dose = CreateDose(scheduledDate: DateTime.UtcNow.AddHours(-5.5));

        Assert.Equal(5, dose.CalculateHoursWithoutConfirmation());
    }

    [Fact]
    public void CalculateHoursWithoutConfirmation_WhenOmitted_ReturnsWholeHoursSinceSchedule()
    {
        var dose = CreateDose(scheduledDate: DateTime.UtcNow.AddHours(-3.5), status: DoseStatus.OMITTED);

        Assert.Equal(3, dose.CalculateHoursWithoutConfirmation());
    }

    [Fact]
    public void CalculateHoursWithoutConfirmation_WhenLessThanOneHour_ReturnsZero()
    {
        var dose = CreateDose(scheduledDate: DateTime.UtcNow.AddMinutes(-30));

        Assert.Equal(0, dose.CalculateHoursWithoutConfirmation());
    }

    // ==========================================
    // Consultas de estado
    // ==========================================

    [Theory]
    [InlineData(DoseStatus.PENDING, true, false, false)]
    [InlineData(DoseStatus.CONFIRMED, false, true, false)]
    [InlineData(DoseStatus.OMITTED, false, false, true)]
    public void StatusQueries_ReturnExpectedValuesForStatus(
        DoseStatus status, bool isPending, bool isConfirmed, bool isOmitted)
    {
        var dose = CreateDose(status: status);

        Assert.Equal(isPending, dose.IsPending());
        Assert.Equal(isConfirmed, dose.IsConfirmed());
        Assert.Equal(isOmitted, dose.IsOmitted());
    }

    [Fact]
    public void StatusQueries_AfterConfirm_ReflectConfirmedState()
    {
        var dose = CreateDose();

        dose.Confirm();

        Assert.True(dose.IsConfirmed());
        Assert.False(dose.IsPending());
        Assert.False(dose.IsOmitted());
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var scheduled = DateTime.UtcNow.AddHours(-2);
        var confirmed = DateTime.UtcNow.AddHours(-1);
        var dose = new DailyDose("dose-001", "treatment-001", scheduled, confirmed, DoseStatus.CONFIRMED);

        var primitives = dose.ToPrimitives();

        Assert.Equal(dose.Id, primitives.Id);
        Assert.Equal(dose.TreatmentId, primitives.TreatmentId);
        Assert.Equal(dose.ScheduledDate, primitives.ScheduledDate);
        Assert.Equal(dose.ConfirmedAt, primitives.ConfirmedAt);
        Assert.Equal(dose.Status.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_ForPendingDose_ReturnsNullConfirmedAt()
    {
        var dose = CreateDose();

        var primitives = dose.ToPrimitives();

        Assert.Null(primitives.ConfirmedAt);
        Assert.Equal(DoseStatus.PENDING.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_AfterConfirm_ReflectsStatusAndTimestamp()
    {
        var dose = CreateDose();
        dose.Confirm();

        var primitives = dose.ToPrimitives();

        Assert.Equal(DoseStatus.CONFIRMED.ToStringValue(), primitives.Status);
        Assert.NotNull(primitives.ConfirmedAt);
    }

    [Fact]
    public void ToPrimitives_AfterMarkAsOmitted_ReflectsOmittedStatus()
    {
        var dose = CreateDose();
        dose.MarkAsOmitted();

        var primitives = dose.ToPrimitives();

        Assert.Equal(DoseStatus.OMITTED.ToStringValue(), primitives.Status);
        Assert.Null(primitives.ConfirmedAt);
    }
}
