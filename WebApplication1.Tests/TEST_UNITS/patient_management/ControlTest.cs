using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using Xunit;
using static WebApplication1.Tests.PatientManagement.PatientManagementTestData;

namespace WebApplication1.Tests.PatientManagement;

public class ControlTests
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesControlSuccessfully()
    {
        var date = PastDate();
        var hemoglobin = NewHemoglobin(12.5);

        var control = new Control("control-001", date, hemoglobin);

        Assert.NotNull(control);
        Assert.Equal("control-001", control.Id);
        Assert.Equal(date, control.Date);
        Assert.Same(hemoglobin, control.HemoglobinLevel);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewControl(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultDate_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Control("control-001", default, NewHemoglobin()));

        Assert.Equal("date", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithFutureDate_ThrowsArgumentException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        var ex = Assert.Throws<ArgumentException>(() =>
            new Control("control-001", futureDate, NewHemoglobin()));

        Assert.Equal("date", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHemoglobinLevel_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Control("control-001", PastDate(), null!));

        Assert.Equal("hemoglobinLevel", ex.ParamName);
    }

    // ==========================================
    // AnemiaStatus (se calcula en el constructor)
    // ==========================================

    [Theory]
    [InlineData(3.0, AnemiaStatus.Severe)]
    [InlineData(6.9, AnemiaStatus.Severe)]
    [InlineData(7.0, AnemiaStatus.Moderate)]
    [InlineData(8.0, AnemiaStatus.Moderate)]
    [InlineData(8.9, AnemiaStatus.Moderate)]
    [InlineData(9.0, AnemiaStatus.Mild)]
    [InlineData(10.0, AnemiaStatus.Mild)]
    [InlineData(10.9, AnemiaStatus.Mild)]
    [InlineData(11.0, AnemiaStatus.Controlled)]
    [InlineData(13.5, AnemiaStatus.Controlled)]
    public void Constructor_CalculatesAnemiaStatusFromHemoglobin(double hemoglobin, AnemiaStatus expected)
    {
        var control = NewControl(hemoglobin: hemoglobin);

        Assert.Equal(expected, control.AnemiaStatus);
    }

    [Fact]
    public void Constructor_WithHemoglobinWithoutValue_SetsAnemiaStatusToControlled()
    {
        var control = NewControl(hemoglobin: null);

        Assert.Equal(AnemiaStatus.Controlled, control.AnemiaStatus);
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var control = NewControl(hemoglobin: 8.0);

        var primitives = control.ToPrimitives();

        Assert.Equal(control.Id, primitives.Id);
        Assert.Equal(control.Date, primitives.Date);
        Assert.Equal(control.HemoglobinLevel.Value, primitives.HemoglobinLevel);
        Assert.Equal(control.AnemiaStatus.ToStringValue(), primitives.AnemiaStatus);
    }

    [Fact]
    public void ToPrimitives_ConvertsAnemiaStatusToItsStringValue()
    {
        var control = NewControl(hemoglobin: 6.0);

        var primitives = control.ToPrimitives();

        Assert.Equal(AnemiaStatus.Severe.ToStringValue(), primitives.AnemiaStatus);
    }
}
