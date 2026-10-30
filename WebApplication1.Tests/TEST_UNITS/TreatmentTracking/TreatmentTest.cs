using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using Xunit;

namespace WebApplication1.Tests.TreatmentTracking;

public class TreatmentTests
{
    // ==========================================
    // HELPERS
    // ==========================================
    // El constructor de RiskScore (id, score, nivel, fecha) es el mismo que usa
    // Treatment.Create, asi que no es una suposicion.

    private static readonly DateTime DefaultStart = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

    private static RiskScore NewRiskScore(string id = "risk-001") =>
        new(id, 10, RiskLevel.LOW, DateTime.UtcNow);

    private static Treatment CreateTreatment(
        string id = "treatment-001",
        string patientId = "patient-001",
        string nurseId = "nurse-001",
        string supplement = "Sulfato ferroso",
        string quantity = "1 tableta",
        string dosingHours = "08:00",
        int durationDays = 30,
        DateTime? startDate = null,
        DateTime? endDate = null,
        TreatmentStatus status = TreatmentStatus.ACTIVE,
        double adherenceScore = 100,
        int currentStreak = 0,
        int totalConfirmed = 0,
        int totalOmitted = 0,
        string? completionObservation = null,
        string? abandonmentObservation = null,
        RiskScore? riskScore = null)
    {
        var start = startDate ?? DefaultStart;
        var end = endDate ?? start.AddDays(30);

        return new Treatment(
            id,
            patientId,
            nurseId,
            supplement,
            quantity,
            dosingHours,
            durationDays,
            start,
            end,
            status,
            adherenceScore,
            currentStreak,
            totalConfirmed,
            totalOmitted,
            completionObservation,
            abandonmentObservation,
            riskScore ?? NewRiskScore());
    }

    // ==========================================
    // CONSTRUCTOR
    // ==========================================
    // Nota: Treatment valida con nameof(Propiedad), por eso el ParamName va con
    // mayuscula inicial ("Id", "PatientId", ...) y no como el parametro del constructor.

    [Fact]
    public void Constructor_WithValidData_CreatesTreatmentSuccessfully()
    {
        var start = DefaultStart;
        var end = DefaultStart.AddDays(30);
        var risk = NewRiskScore();

        var treatment = new Treatment(
            "treatment-001", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00",
            30, start, end, TreatmentStatus.ACTIVE, 90.5, 3, 9, 1, "ok", "ninguna", risk);

        Assert.NotNull(treatment);
        Assert.Equal("treatment-001", treatment.Id);
        Assert.Equal("patient-001", treatment.PatientId);
        Assert.Equal("nurse-001", treatment.NurseId);
        Assert.Equal("Sulfato ferroso", treatment.Supplement);
        Assert.Equal("1 tableta", treatment.Quantity);
        Assert.Equal("08:00", treatment.DosingHours);
        Assert.Equal(30, treatment.DurationDays);
        Assert.Equal(start, treatment.StartDate);
        Assert.Equal(end, treatment.EndDate);
        Assert.Equal(TreatmentStatus.ACTIVE, treatment.Status);
        Assert.Equal(90.5, treatment.AdherenceScore);
        Assert.Equal(3, treatment.CurrentStreak);
        Assert.Equal(9, treatment.TotalConfirmed);
        Assert.Equal(1, treatment.TotalOmitted);
        Assert.Equal("ok", treatment.CompletionObservation);
        Assert.Equal("ninguna", treatment.AbandonmentObservation);
        Assert.Equal(risk, treatment.RiskScore);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(id: invalid!));

        Assert.Equal("Id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPatientId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(patientId: invalid!));

        Assert.Equal("PatientId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidNurseId_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(nurseId: invalid!));

        Assert.Equal("NurseId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSupplement_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(supplement: invalid!));

        Assert.Equal("Supplement", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidQuantity_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(quantity: invalid!));

        Assert.Equal("Quantity", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidDosingHours_ThrowsArgumentException(string? invalid)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => CreateTreatment(dosingHours: invalid!));

        Assert.Equal("DosingHours", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Constructor_WithDurationZeroOrNegative_ThrowsArgumentException(int invalidDuration)
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateTreatment(durationDays: invalidDuration));

        Assert.Equal("DurationDays", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultStartDate_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateTreatment(startDate: default(DateTime)));

        Assert.Equal("StartDate", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultEndDate_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateTreatment(endDate: default(DateTime)));

        Assert.Equal("EndDate", ex.ParamName);
    }

    // ==========================================
    // FACTORY METHOD: Create
    // ==========================================

    [Fact]
    public void Create_WithValidData_BuildsActiveTreatmentWithInitialValues()
    {
        var treatment = Treatment.Create(
            "treatment-001", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00", 30);

        Assert.Equal("treatment-001", treatment.Id);
        Assert.Equal("patient-001", treatment.PatientId);
        Assert.Equal("nurse-001", treatment.NurseId);
        Assert.Equal("Sulfato ferroso", treatment.Supplement);
        Assert.Equal("1 tableta", treatment.Quantity);
        Assert.Equal("08:00", treatment.DosingHours);
        Assert.Equal(30, treatment.DurationDays);
        Assert.Equal(TreatmentStatus.ACTIVE, treatment.Status);
        Assert.Equal(100, treatment.AdherenceScore);
        Assert.Equal(0, treatment.CurrentStreak);
        Assert.Equal(0, treatment.TotalConfirmed);
        Assert.Equal(0, treatment.TotalOmitted);
        Assert.Null(treatment.CompletionObservation);
        Assert.Null(treatment.AbandonmentObservation);
        Assert.NotNull(treatment.RiskScore);
    }

    [Fact]
    public void Create_SetsStartDateToNowAndEndDateAfterDuration()
    {
        var before = DateTime.UtcNow;

        var treatment = Treatment.Create(
            "treatment-001", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00", 7);

        Assert.InRange(treatment.StartDate, before, DateTime.UtcNow);
        Assert.Equal(treatment.StartDate.AddDays(7), treatment.EndDate);
    }

    [Fact]
    public void Create_WithInvalidDuration_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => Treatment.Create(
            "treatment-001", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00", 0));

        Assert.Equal("DurationDays", ex.ParamName);
    }

    [Fact]
    public void Create_WithInvalidId_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => Treatment.Create(
            "  ", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00", 30));

        Assert.Equal("Id", ex.ParamName);
    }

    // ==========================================
    // CompleteTreatment
    // ==========================================

    [Fact]
    public void CompleteTreatment_WithAssignedNurse_CompletesTreatmentAndStoresObservation()
    {
        var treatment = CreateTreatment();

        treatment.CompleteTreatment("nurse-001", "Tratamiento terminado sin problemas");

        Assert.Equal(TreatmentStatus.COMPLETED, treatment.Status);
        Assert.Equal("Tratamiento terminado sin problemas", treatment.CompletionObservation);
    }

    [Fact]
    public void CompleteTreatment_WithoutObservation_LeavesObservationNull()
    {
        var treatment = CreateTreatment();

        treatment.CompleteTreatment("nurse-001");

        Assert.Equal(TreatmentStatus.COMPLETED, treatment.Status);
        Assert.Null(treatment.CompletionObservation);
    }

    [Fact]
    public void CompleteTreatment_WithDifferentNurse_ThrowsInvalidOperationException()
    {
        var treatment = CreateTreatment();

        Assert.Throws<InvalidOperationException>(() => treatment.CompleteTreatment("nurse-999"));
    }

    [Fact]
    public void CompleteTreatment_WithDifferentNurse_DoesNotChangeState()
    {
        var treatment = CreateTreatment();

        Assert.Throws<InvalidOperationException>(() => treatment.CompleteTreatment("nurse-999", "obs"));

        Assert.Equal(TreatmentStatus.ACTIVE, treatment.Status);
        Assert.Null(treatment.CompletionObservation);
    }

    [Theory]
    [InlineData(TreatmentStatus.COMPLETED)]
    [InlineData(TreatmentStatus.ABANDONED)]
    public void CompleteTreatment_WhenNotActive_ThrowsInvalidOperationException(TreatmentStatus status)
    {
        var treatment = CreateTreatment(status: status);

        Assert.Throws<InvalidOperationException>(() => treatment.CompleteTreatment("nurse-001"));
    }

    // ==========================================
    // AbandonTreatment
    // ==========================================

    [Fact]
    public void AbandonTreatment_WithAssignedNurse_AbandonsTreatmentAndStoresObservation()
    {
        var treatment = CreateTreatment();

        treatment.AbandonTreatment("nurse-001", "El paciente dejo de asistir");

        Assert.Equal(TreatmentStatus.ABANDONED, treatment.Status);
        Assert.Equal("El paciente dejo de asistir", treatment.AbandonmentObservation);
    }

    [Fact]
    public void AbandonTreatment_WithoutObservation_LeavesObservationNull()
    {
        var treatment = CreateTreatment();

        treatment.AbandonTreatment("nurse-001");

        Assert.Equal(TreatmentStatus.ABANDONED, treatment.Status);
        Assert.Null(treatment.AbandonmentObservation);
    }

    [Fact]
    public void AbandonTreatment_WithDifferentNurse_ThrowsInvalidOperationException()
    {
        var treatment = CreateTreatment();

        Assert.Throws<InvalidOperationException>(() => treatment.AbandonTreatment("nurse-999"));
    }

    [Fact]
    public void AbandonTreatment_WithDifferentNurse_DoesNotChangeState()
    {
        var treatment = CreateTreatment();

        Assert.Throws<InvalidOperationException>(() => treatment.AbandonTreatment("nurse-999", "obs"));

        Assert.Equal(TreatmentStatus.ACTIVE, treatment.Status);
        Assert.Null(treatment.AbandonmentObservation);
    }

    [Theory]
    [InlineData(TreatmentStatus.COMPLETED)]
    [InlineData(TreatmentStatus.ABANDONED)]
    public void AbandonTreatment_WhenNotActive_ThrowsInvalidOperationException(TreatmentStatus status)
    {
        var treatment = CreateTreatment(status: status);

        Assert.Throws<InvalidOperationException>(() => treatment.AbandonTreatment("nurse-001"));
    }

    // ==========================================
    // UpdateAdherenceMetrics
    // ==========================================

    [Fact]
    public void UpdateAdherenceMetrics_WhenConfirmed_IncrementsConfirmedAndStreak()
    {
        var treatment = CreateTreatment();

        treatment.UpdateAdherenceMetrics(true);

        Assert.Equal(1, treatment.TotalConfirmed);
        Assert.Equal(0, treatment.TotalOmitted);
        Assert.Equal(1, treatment.CurrentStreak);
        Assert.Equal(100, treatment.AdherenceScore);
    }

    [Fact]
    public void UpdateAdherenceMetrics_WhenOmitted_IncrementsOmittedAndResetsStreak()
    {
        var treatment = CreateTreatment(currentStreak: 5, totalConfirmed: 5);

        treatment.UpdateAdherenceMetrics(false);

        Assert.Equal(5, treatment.TotalConfirmed);
        Assert.Equal(1, treatment.TotalOmitted);
        Assert.Equal(0, treatment.CurrentStreak);
    }

    [Fact]
    public void UpdateAdherenceMetrics_WhenOnlyOmitted_SetsScoreToZero()
    {
        var treatment = CreateTreatment();

        treatment.UpdateAdherenceMetrics(false);

        Assert.Equal(0, treatment.AdherenceScore);
    }

    [Fact]
    public void UpdateAdherenceMetrics_CalculatesScoreAsPercentageOfConfirmed()
    {
        var treatment = CreateTreatment();

        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(false);

        Assert.Equal(3, treatment.TotalConfirmed);
        Assert.Equal(1, treatment.TotalOmitted);
        Assert.Equal(75, treatment.AdherenceScore);
    }

    [Fact]
    public void UpdateAdherenceMetrics_WithNonExactPercentage_CalculatesScoreWithDecimals()
    {
        var treatment = CreateTreatment();

        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(false);

        Assert.Equal(66.67, treatment.AdherenceScore, 2);
    }

    [Fact]
    public void UpdateAdherenceMetrics_AfterOmission_RestartsStreakFromOne()
    {
        var treatment = CreateTreatment();
        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(true);
        treatment.UpdateAdherenceMetrics(false);

        treatment.UpdateAdherenceMetrics(true);

        Assert.Equal(1, treatment.CurrentStreak);
        Assert.Equal(3, treatment.TotalConfirmed);
        Assert.Equal(1, treatment.TotalOmitted);
    }

    // ==========================================
    // UpdateRiskScore
    // ==========================================

    [Fact]
    public void UpdateRiskScore_ReplacesCurrentRiskScore()
    {
        var treatment = CreateTreatment();
        var newRisk = new RiskScore("risk-002", 55, RiskLevel.LOW, DateTime.UtcNow);

        treatment.UpdateRiskScore(newRisk);

        Assert.Equal(newRisk, treatment.RiskScore);
    }

    // ==========================================
    // Consultas de estado
    // ==========================================

    [Theory]
    [InlineData(TreatmentStatus.ACTIVE, true, false, false)]
    [InlineData(TreatmentStatus.COMPLETED, false, true, false)]
    [InlineData(TreatmentStatus.ABANDONED, false, false, true)]
    public void StatusQueries_ReturnExpectedValuesForStatus(
        TreatmentStatus status, bool isActive, bool isCompleted, bool isAbandoned)
    {
        var treatment = CreateTreatment(status: status);

        Assert.Equal(isActive, treatment.IsActive());
        Assert.Equal(isCompleted, treatment.IsCompleted());
        Assert.Equal(isAbandoned, treatment.IsAbandoned());
    }

    [Fact]
    public void StatusQueries_AfterCompleteTreatment_ReflectCompletedState()
    {
        var treatment = CreateTreatment();

        treatment.CompleteTreatment("nurse-001");

        Assert.True(treatment.IsCompleted());
        Assert.False(treatment.IsActive());
        Assert.False(treatment.IsAbandoned());
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var risk = NewRiskScore();
        var treatment = CreateTreatment(
            adherenceScore: 80,
            currentStreak: 2,
            totalConfirmed: 8,
            totalOmitted: 2,
            completionObservation: "obs completado",
            abandonmentObservation: "obs abandono",
            riskScore: risk);

        var primitives = treatment.ToPrimitives();

        Assert.Equal(treatment.Id, primitives.Id);
        Assert.Equal(treatment.PatientId, primitives.PatientId);
        Assert.Equal(treatment.NurseId, primitives.NurseId);
        Assert.Equal(treatment.Supplement, primitives.Supplement);
        Assert.Equal(treatment.Quantity, primitives.Quantity);
        Assert.Equal(treatment.DosingHours, primitives.DosingHours);
        Assert.Equal(treatment.DurationDays, primitives.DurationDays);
        Assert.Equal(treatment.StartDate, primitives.StartDate);
        Assert.Equal(treatment.EndDate, primitives.EndDate);
        Assert.Equal(treatment.Status.ToStringValue(), primitives.Status);
        Assert.Equal(80, primitives.AdherenceScore);
        Assert.Equal(2, primitives.CurrentStreak);
        Assert.Equal(8, primitives.TotalConfirmed);
        Assert.Equal(2, primitives.TotalOmitted);
        Assert.Equal("obs completado", primitives.CompletionObservation);
        Assert.Equal("obs abandono", primitives.AbandonmentObservation);
        Assert.NotNull(primitives.RiskScore);
        Assert.Equivalent(risk.ToPrimitives(), primitives.RiskScore);
    }

    [Fact]
    public void ToPrimitives_AfterCompleteTreatment_ReflectsStatusAndObservation()
    {
        var treatment = CreateTreatment();
        treatment.CompleteTreatment("nurse-001", "Listo");

        var primitives = treatment.ToPrimitives();

        Assert.Equal(TreatmentStatus.COMPLETED.ToStringValue(), primitives.Status);
        Assert.Equal("Listo", primitives.CompletionObservation);
        Assert.Null(primitives.AbandonmentObservation);
    }

    [Fact]
    public void ToPrimitives_ForNewTreatment_HasNullObservations()
    {
        var treatment = Treatment.Create(
            "treatment-001", "patient-001", "nurse-001", "Sulfato ferroso", "1 tableta", "08:00", 30);

        var primitives = treatment.ToPrimitives();

        Assert.Null(primitives.CompletionObservation);
        Assert.Null(primitives.AbandonmentObservation);
        Assert.Equal(TreatmentStatus.ACTIVE.ToStringValue(), primitives.Status);
    }
}
