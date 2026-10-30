using WebApplication1.NutritionDiary.Domain.Models.Aggregate;
using Xunit;

namespace WebApplication1.Tests.NutritionDiary.Domain.Models.Aggregate;

public class NutritionalDiaryTests
{
    // ==========================================
    // HELPERS
    // ==========================================

    private static readonly DateTime TestDate = new(2026, 10, 24, 8, 0, 0, DateTimeKind.Utc);

    private static NutritionalDiary CreateDiary(
        string id = "diary-001",
        string patientId = "patient-001",
        string motherId = "mother-001",
        DateTime? date = null,
        double totalIronAbsorbed = 5.5,
        bool hasInhibitor = false)
    {
        return new NutritionalDiary(
            id,
            patientId,
            motherId,
            date ?? TestDate,
            totalIronAbsorbed,
            hasInhibitor);
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesDiarySuccessfully()
    {
        var diary = new NutritionalDiary("diary-001", "patient-001", "mother-001", TestDate, 5.5, true);

        Assert.NotNull(diary);
        Assert.Equal("diary-001", diary.Id);
        Assert.Equal("patient-001", diary.PatientId);
        Assert.Equal("mother-001", diary.MotherId);
        Assert.Equal(TestDate, diary.Date);
        Assert.Equal(5.5, diary.TotalIronAbsorbed);
        Assert.True(diary.HasInhibitor);
    }

    [Fact]
    public void Constructor_WithZeroIron_CreatesDiarySuccessfully()
    {
        var diary = CreateDiary(totalIronAbsorbed: 0);

        Assert.Equal(0, diary.TotalIronAbsorbed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateDiary(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPatientId_ThrowsArgumentException(string? invalidPatientId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateDiary(patientId: invalidPatientId!));

        Assert.Equal("patientId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMotherId_ThrowsArgumentException(string? invalidMotherId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateDiary(motherId: invalidMotherId!));

        Assert.Equal("motherId", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultDate_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateDiary(date: default(DateTime)));

        Assert.Equal("date", ex.ParamName);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-100.5)]
    public void Constructor_WithNegativeIron_ThrowsArgumentException(double negativeIron)
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateDiary(totalIronAbsorbed: negativeIron));

        Assert.Equal("totalIronAbsorbed", ex.ParamName);
    }

    // ==========================================
    // UpdateMetrics
    // ==========================================

    [Fact]
    public void UpdateMetrics_WithValidData_UpdatesIronAndInhibitor()
    {
        var diary = CreateDiary(totalIronAbsorbed: 5.5, hasInhibitor: false);

        diary.UpdateMetrics(8.2, true);

        Assert.Equal(8.2, diary.TotalIronAbsorbed);
        Assert.True(diary.HasInhibitor);
    }

    [Fact]
    public void UpdateMetrics_WithZeroIron_UpdatesSuccessfully()
    {
        var diary = CreateDiary(totalIronAbsorbed: 5.5);

        diary.UpdateMetrics(0, false);

        Assert.Equal(0, diary.TotalIronAbsorbed);
        Assert.False(diary.HasInhibitor);
    }

    [Fact]
    public void UpdateMetrics_WithNegativeIron_ThrowsArgumentException()
    {
        var diary = CreateDiary();

        var ex = Assert.Throws<ArgumentException>(() => diary.UpdateMetrics(-1, true));

        Assert.Equal("totalIronAbsorbed", ex.ParamName);
    }

    [Fact]
    public void UpdateMetrics_WithNegativeIron_DoesNotChangeExistingState()
    {
        var diary = CreateDiary(totalIronAbsorbed: 5.5, hasInhibitor: false);

        Assert.Throws<ArgumentException>(() => diary.UpdateMetrics(-1, true));

        Assert.Equal(5.5, diary.TotalIronAbsorbed);
        Assert.False(diary.HasInhibitor);
    }

    // ==========================================
    // MarkInhibitorDetected
    // ==========================================

    [Fact]
    public void MarkInhibitorDetected_SetsHasInhibitorToTrue()
    {
        var diary = CreateDiary(hasInhibitor: false);

        diary.MarkInhibitorDetected();

        Assert.True(diary.HasInhibitor);
    }

    [Fact]
    public void MarkInhibitorDetected_WhenAlreadyTrue_KeepsItTrue()
    {
        var diary = CreateDiary(hasInhibitor: true);

        diary.MarkInhibitorDetected();

        Assert.True(diary.HasInhibitor);
    }

    [Fact]
    public void MarkInhibitorDetected_DoesNotChangeIron()
    {
        var diary = CreateDiary(totalIronAbsorbed: 5.5);

        diary.MarkInhibitorDetected();

        Assert.Equal(5.5, diary.TotalIronAbsorbed);
    }

    // ==========================================
    // ResetDailyIron
    // ==========================================

    [Fact]
    public void ResetDailyIron_SetsIronToZeroAndInhibitorToFalse()
    {
        var diary = CreateDiary(totalIronAbsorbed: 7.3, hasInhibitor: true);

        diary.ResetDailyIron();

        Assert.Equal(0, diary.TotalIronAbsorbed);
        Assert.False(diary.HasInhibitor);
    }

    [Fact]
    public void ResetDailyIron_DoesNotChangeIdentityFields()
    {
        var diary = CreateDiary();

        diary.ResetDailyIron();

        Assert.Equal("diary-001", diary.Id);
        Assert.Equal("patient-001", diary.PatientId);
        Assert.Equal("mother-001", diary.MotherId);
        Assert.Equal(TestDate, diary.Date);
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var diary = CreateDiary(totalIronAbsorbed: 6.4, hasInhibitor: true);

        var primitives = diary.ToPrimitives();

        Assert.Equal(diary.Id, primitives.Id);
        Assert.Equal(diary.PatientId, primitives.PatientId);
        Assert.Equal(diary.MotherId, primitives.MotherId);
        Assert.Equal(diary.Date, primitives.Date);
        Assert.Equal(diary.TotalIronAbsorbed, primitives.TotalIronAbsorbed);
        Assert.Equal(diary.HasInhibitor, primitives.HasInhibitor);
    }

    [Fact]
    public void ToPrimitives_AfterUpdateMetrics_ReflectsNewValues()
    {
        var diary = CreateDiary();
        diary.UpdateMetrics(9.9, true);

        var primitives = diary.ToPrimitives();

        Assert.Equal(9.9, primitives.TotalIronAbsorbed);
        Assert.True(primitives.HasInhibitor);
    }
}
