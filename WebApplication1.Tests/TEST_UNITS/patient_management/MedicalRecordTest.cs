using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;
using Xunit;
using static WebApplication1.Tests.PatientManagement.PatientManagementTestData;

namespace WebApplication1.Tests.PatientManagement;

public class MedicalRecordTests
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesRecordSuccessfully()
    {
        var createdAt = PastDate();
        var weight = NewWeight(9.2);
        var height = NewHeight(72);
        var gender = AnyGender();
        var motivo = NewMotivoConsulta();
        var observaciones = NewObservaciones();

        var record = new MedicalRecord(
            "record-001", createdAt, weight, height, gender, motivo, observaciones,
            "patient-001", "nurse-001");

        Assert.NotNull(record);
        Assert.Equal("record-001", record.Id);
        Assert.Equal(createdAt, record.CreatedAt);
        Assert.Same(weight, record.Weight);
        Assert.Same(height, record.Height);
        Assert.Equal(gender, record.Gender);
        Assert.Same(motivo, record.MotivoConsulta);
        Assert.Same(observaciones, record.Observaciones);
        Assert.Equal("patient-001", record.PatientId);
        Assert.Equal("nurse-001", record.NurseId);
    }

    [Fact]
    public void Constructor_SetsUpdatedAtEqualToCreatedAt()
    {
        var createdAt = PastDate();

        var record = NewMedicalRecord(createdAt: createdAt);

        Assert.Equal(createdAt, record.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithoutOptionalData_UsesEmptyCollectionsAndNulls()
    {
        var record = NewMedicalRecord();

        Assert.Null(record.NurseId);
        Assert.Null(record.HemoglobinLevel);
        Assert.NotNull(record.Antecedentes);
        Assert.Empty(record.Antecedentes);
        Assert.NotNull(record.Sintomas);
        Assert.Empty(record.Sintomas);
        Assert.NotNull(record.Controls);
        Assert.Empty(record.Controls);
    }

    [Fact]
    public void Constructor_WithOptionalData_StoresIt()
    {
        var hemoglobin = NewHemoglobin(10.5);
        var antecedentes = new List<Antecedente> { NewAntecedente("Prematuridad") };
        var sintomas = new List<string> { "Palidez", "Cansancio" };
        var controls = new List<Control> { NewControl() };

        var record = NewMedicalRecord(
            hemoglobinLevel: hemoglobin,
            antecedentes: antecedentes,
            sintomas: sintomas,
            controls: controls);

        Assert.Same(hemoglobin, record.HemoglobinLevel);
        Assert.Single(record.Antecedentes);
        Assert.Equal(2, record.Sintomas.Count);
        Assert.Single(record.Controls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewMedicalRecord(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPatientId_ThrowsArgumentException(string? invalidPatientId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewMedicalRecord(patientId: invalidPatientId!));

        Assert.Equal("patientId", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullWeight_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MedicalRecord(
            "record-001", PastDate(), null!, NewHeight(), AnyGender(),
            NewMotivoConsulta(), NewObservaciones(), "patient-001"));

        Assert.Equal("weight", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHeight_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MedicalRecord(
            "record-001", PastDate(), NewWeight(), null!, AnyGender(),
            NewMotivoConsulta(), NewObservaciones(), "patient-001"));

        Assert.Equal("height", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMotivoConsulta_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MedicalRecord(
            "record-001", PastDate(), NewWeight(), NewHeight(), AnyGender(),
            null!, NewObservaciones(), "patient-001"));

        Assert.Equal("motivoConsulta", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullObservaciones_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MedicalRecord(
            "record-001", PastDate(), NewWeight(), NewHeight(), AnyGender(),
            NewMotivoConsulta(), null!, "patient-001"));

        Assert.Equal("observaciones", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithDuplicateAntecedentes_ThrowsInvalidOperationException()
    {
        var duplicated = new List<Antecedente>
        {
            NewAntecedente("Prematuridad"),
            NewAntecedente("Prematuridad")
        };

        Assert.Throws<InvalidOperationException>(() => NewMedicalRecord(antecedentes: duplicated));
    }

    [Fact]
    public void Constructor_WithDifferentAntecedentes_CreatesRecordSuccessfully()
    {
        var antecedentes = new List<Antecedente>
        {
            NewAntecedente("Prematuridad"),
            NewAntecedente("BajoPeso")
        };

        var record = NewMedicalRecord(antecedentes: antecedentes);

        Assert.Equal(2, record.Antecedentes.Count);
    }

    // ==========================================
    // AddControl
    // ==========================================

    [Fact]
    public void AddControl_WithValidControl_AddsItToControls()
    {
        var record = NewMedicalRecord();
        var control = NewControl();

        record.AddControl(control);

        Assert.Single(record.Controls);
        Assert.Same(control, record.Controls[0]);
    }

    [Fact]
    public void AddControl_UpdatesHemoglobinLevelWithLatestControl()
    {
        var record = NewMedicalRecord();
        var first = NewControl(id: "control-001", hemoglobin: 8.0);
        var second = NewControl(id: "control-002", hemoglobin: 11.5);

        record.AddControl(first);
        record.AddControl(second);

        Assert.Equal(2, record.Controls.Count);
        Assert.Same(second.HemoglobinLevel, record.HemoglobinLevel);
    }

    [Fact]
    public void AddControl_UpdatesUpdatedAt()
    {
        var record = NewMedicalRecord(createdAt: PastDate());
        var before = DateTime.UtcNow;

        record.AddControl(NewControl());

        Assert.InRange(record.UpdatedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public void AddControl_WithNull_ThrowsArgumentNullException()
    {
        var record = NewMedicalRecord();

        var ex = Assert.Throws<ArgumentNullException>(() => record.AddControl(null!));

        Assert.Equal("control", ex.ParamName);
    }

    // ==========================================
    // UpdateClinicalInformation
    // ==========================================

    [Fact]
    public void UpdateClinicalInformation_WithAllValues_UpdatesEveryField()
    {
        var record = NewMedicalRecord();
        var newWeight = NewWeight(10.1);
        var newHeight = NewHeight(75);
        var newMotivo = NewMotivoConsulta("Seguimiento");
        var newObservaciones = NewObservaciones("Mejora");
        var newAntecedentes = new List<Antecedente> { NewAntecedente("Prematuridad") };
        var newSintomas = new List<string> { "Palidez" };

        record.UpdateClinicalInformation(
            newWeight, newHeight, newMotivo, newObservaciones, newAntecedentes, newSintomas);

        Assert.Same(newWeight, record.Weight);
        Assert.Same(newHeight, record.Height);
        Assert.Same(newMotivo, record.MotivoConsulta);
        Assert.Same(newObservaciones, record.Observaciones);
        Assert.Same(newAntecedentes, record.Antecedentes);
        Assert.Same(newSintomas, record.Sintomas);
    }

    [Fact]
    public void UpdateClinicalInformation_WithOnlyWeight_KeepsOtherFields()
    {
        var record = NewMedicalRecord();
        var originalHeight = record.Height;
        var originalMotivo = record.MotivoConsulta;
        var originalObservaciones = record.Observaciones;
        var newWeight = NewWeight(10.1);

        record.UpdateClinicalInformation(weight: newWeight);

        Assert.Same(newWeight, record.Weight);
        Assert.Same(originalHeight, record.Height);
        Assert.Same(originalMotivo, record.MotivoConsulta);
        Assert.Same(originalObservaciones, record.Observaciones);
    }

    [Fact]
    public void UpdateClinicalInformation_AlwaysUpdatesUpdatedAt()
    {
        var record = NewMedicalRecord(createdAt: PastDate());
        var before = DateTime.UtcNow;

        record.UpdateClinicalInformation();

        Assert.InRange(record.UpdatedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateClinicalInformation_WithDuplicateAntecedentes_ThrowsInvalidOperationException()
    {
        var record = NewMedicalRecord();
        var duplicated = new List<Antecedente>
        {
            NewAntecedente("Prematuridad"),
            NewAntecedente("Prematuridad")
        };

        Assert.Throws<InvalidOperationException>(() =>
            record.UpdateClinicalInformation(antecedentes: duplicated));
    }

    [Fact]
    public void UpdateClinicalInformation_WithDuplicateAntecedentes_DoesNotChangeExistingState()
    {
        var originalAntecedentes = new List<Antecedente> { NewAntecedente("BajoPeso") };
        var record = NewMedicalRecord(antecedentes: originalAntecedentes);
        var originalUpdatedAt = record.UpdatedAt;
        var duplicated = new List<Antecedente>
        {
            NewAntecedente("Prematuridad"),
            NewAntecedente("Prematuridad")
        };

        Assert.Throws<InvalidOperationException>(() =>
            record.UpdateClinicalInformation(antecedentes: duplicated));

        Assert.Same(originalAntecedentes, record.Antecedentes);
        Assert.Equal(originalUpdatedAt, record.UpdatedAt);
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var record = NewMedicalRecord(
            nurseId: "nurse-001",
            hemoglobinLevel: NewHemoglobin(10.5),
            antecedentes: new List<Antecedente> { NewAntecedente("Prematuridad") },
            sintomas: new List<string> { "Palidez" },
            controls: new List<Control> { NewControl() });

        var primitives = record.ToPrimitives();

        Assert.Equal(record.Id, primitives.Id);
        Assert.Equal(record.CreatedAt, primitives.CreatedAt);
        Assert.Equal(record.UpdatedAt, primitives.UpdatedAt);
        Assert.Equal(record.HemoglobinLevel!.Value, primitives.HemoglobinLevel);
        Assert.Equal(record.Weight.Value, primitives.Weight);
        Assert.Equal(record.Height.Value, primitives.Height);
        Assert.Equal(record.Gender.ToStringValue(), primitives.Gender);
        Assert.Equal(record.MotivoConsulta.Value, primitives.MotivoConsulta);
        Assert.Equal(record.PatientId, primitives.PatientId);
        Assert.Equal("nurse-001", primitives.NurseId);
        Assert.Single(primitives.Antecedentes);
        Assert.Equal(record.Sintomas, primitives.Sintomas);
        Assert.Single(primitives.Controls);
    }

    [Fact]
    public void ToPrimitives_WithoutHemoglobinLevel_ReturnsNullHemoglobin()
    {
        var record = NewMedicalRecord();

        var primitives = record.ToPrimitives();

        Assert.Null(primitives.HemoglobinLevel);
        Assert.Null(primitives.NurseId);
        Assert.Empty(primitives.Antecedentes);
        Assert.Empty(primitives.Controls);
    }

    [Fact]
    public void ToPrimitives_AfterAddControl_IncludesControlAndHemoglobin()
    {
        var record = NewMedicalRecord();
        var control = NewControl(hemoglobin: 9.5);
        record.AddControl(control);

        var primitives = record.ToPrimitives();

        Assert.Single(primitives.Controls);
        Assert.Equal(control.HemoglobinLevel.Value, primitives.HemoglobinLevel);
    }
}
