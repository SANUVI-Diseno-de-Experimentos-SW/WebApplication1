using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Enums;
using Xunit;
using static WebApplication1.Tests.PatientManagement.PatientManagementTestData;

namespace WebApplication1.Tests.PatientManagement;

public class PatientTests
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    [Fact]
    public void Constructor_WithValidData_CreatesPatientSuccessfully()
    {
        var birthDate = NewBirthDate();
        var weight = NewWeight(8.5);
        var height = NewHeight(70);
        var gender = AnyGender();

        var patient = new Patient(
            "patient-001", "Ana", "Perez", birthDate, weight, height, "mother-001", gender);

        Assert.NotNull(patient);
        Assert.Equal("patient-001", patient.Id);
        Assert.Equal("Ana", patient.Name);
        Assert.Equal("Perez", patient.LastName);
        Assert.Same(birthDate, patient.BirthDate);
        Assert.Same(weight, patient.CurrentWeight);
        Assert.Same(height, patient.CurrentHeight);
        Assert.Equal("mother-001", patient.MotherId);
        Assert.Equal(gender, patient.Gender);
    }

    [Fact]
    public void Constructor_WithoutOptionalData_UsesDefaults()
    {
        var patient = NewPatient();

        Assert.Null(patient.NurseId);
        Assert.Null(patient.FacilityId);
        Assert.Null(patient.MedicalRecord);
        Assert.Equal(PatientStatus.Active, patient.Status);
    }

    [Fact]
    public void Constructor_WithOptionalData_StoresIt()
    {
        var record = NewMedicalRecord();

        var patient = NewPatient(
            nurseId: "nurse-001",
            facilityId: "facility-001",
            status: PatientStatus.Discharged,
            medicalRecord: record);

        Assert.Equal("nurse-001", patient.NurseId);
        Assert.Equal("facility-001", patient.FacilityId);
        Assert.Equal(PatientStatus.Discharged, patient.Status);
        Assert.Same(record, patient.MedicalRecord);
    }

    [Fact]
    public void Constructor_TrimsNameAndLastName()
    {
        var patient = NewPatient(name: "  Ana  ", lastName: "  Perez  ");

        Assert.Equal("Ana", patient.Name);
        Assert.Equal("Perez", patient.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewPatient(id: invalidId!));

        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewPatient(name: invalidName!));

        Assert.Equal("name", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidLastName_ThrowsArgumentException(string? invalidLastName)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewPatient(lastName: invalidLastName!));

        Assert.Equal("lastName", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMotherId_ThrowsArgumentException(string? invalidMotherId)
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() => NewPatient(motherId: invalidMotherId!));

        Assert.Equal("motherId", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullBirthDate_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Patient(
            "patient-001", "Ana", "Perez", null!, NewWeight(), NewHeight(), "mother-001", AnyGender()));

        Assert.Equal("birthDate", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullWeight_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Patient(
            "patient-001", "Ana", "Perez", NewBirthDate(), null!, NewHeight(), "mother-001", AnyGender()));

        Assert.Equal("currentWeight", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHeight_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Patient(
            "patient-001", "Ana", "Perez", NewBirthDate(), NewWeight(), null!, "mother-001", AnyGender()));

        Assert.Equal("currentHeight", ex.ParamName);
    }

    // ==========================================
    // AssignNurse
    // ==========================================

    [Fact]
    public void AssignNurse_WhenPatientHasNoNurse_AssignsNurseAndFacility()
    {
        var patient = NewPatient();

        patient.AssignNurse("nurse-001", "facility-001");

        Assert.Equal("nurse-001", patient.NurseId);
        Assert.Equal("facility-001", patient.FacilityId);
        Assert.Equal(PatientStatus.Active, patient.Status);
    }

    [Fact]
    public void AssignNurse_WhenPatientAlreadyHasNurseAndIsNotDischarged_ThrowsInvalidOperationException()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        Assert.Throws<InvalidOperationException>(() =>
            patient.AssignNurse("nurse-002", "facility-002"));
    }

    [Fact]
    public void AssignNurse_WhenFails_DoesNotChangeExistingAssignment()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        Assert.Throws<InvalidOperationException>(() =>
            patient.AssignNurse("nurse-002", "facility-002"));

        Assert.Equal("nurse-001", patient.NurseId);
        Assert.Equal("facility-001", patient.FacilityId);
    }

    [Fact]
    public void AssignNurse_AfterDischarge_AllowsNewAssignmentAndReactivatesPatient()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");
        patient.Discharge("nurse-001");

        patient.AssignNurse("nurse-002", "facility-002");

        Assert.Equal("nurse-002", patient.NurseId);
        Assert.Equal("facility-002", patient.FacilityId);
        Assert.Equal(PatientStatus.Active, patient.Status);
    }

    [Fact]
    public void AssignNurse_WhenDischargedPatientStillHasNurseId_AllowsAssignment()
    {
        var patient = NewPatient(
            nurseId: "nurse-001",
            facilityId: "facility-001",
            status: PatientStatus.Discharged);

        patient.AssignNurse("nurse-002", "facility-002");

        Assert.Equal("nurse-002", patient.NurseId);
        Assert.Equal(PatientStatus.Active, patient.Status);
    }

    // ==========================================
    // Discharge
    // ==========================================

    [Fact]
    public void Discharge_WithAssignedNurse_DischargesPatientAndClearsAssignment()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        patient.Discharge("nurse-001");

        Assert.Equal(PatientStatus.Discharged, patient.Status);
        Assert.Null(patient.NurseId);
        Assert.Null(patient.FacilityId);
    }

    [Fact]
    public void Discharge_WithDifferentNurse_ThrowsUnauthorizedAccessException()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        Assert.Throws<UnauthorizedAccessException>(() => patient.Discharge("nurse-999"));
    }

    [Fact]
    public void Discharge_WithDifferentNurse_DoesNotChangeState()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        Assert.Throws<UnauthorizedAccessException>(() => patient.Discharge("nurse-999"));

        Assert.Equal(PatientStatus.Active, patient.Status);
        Assert.Equal("nurse-001", patient.NurseId);
        Assert.Equal("facility-001", patient.FacilityId);
    }

    [Fact]
    public void Discharge_WhenPatientHasNoNurse_ThrowsUnauthorizedAccessException()
    {
        var patient = NewPatient();

        Assert.Throws<UnauthorizedAccessException>(() => patient.Discharge("nurse-001"));
    }

    // ==========================================
    // UpdateWeight / UpdateHeight
    // ==========================================

    [Fact]
    public void UpdateWeight_ReplacesCurrentWeight()
    {
        var patient = NewPatient();
        var newWeight = NewWeight(9.9);

        patient.UpdateWeight(newWeight);

        Assert.Same(newWeight, patient.CurrentWeight);
    }

    [Fact]
    public void UpdateHeight_ReplacesCurrentHeight()
    {
        var patient = NewPatient();
        var newHeight = NewHeight(75);

        patient.UpdateHeight(newHeight);

        Assert.Same(newHeight, patient.CurrentHeight);
    }

    // ==========================================
    // MedicalRecord
    // ==========================================

    [Fact]
    public void AssignMedicalRecord_WhenPatientHasNone_AssignsIt()
    {
        var patient = NewPatient();
        var record = NewMedicalRecord();

        patient.AssignMedicalRecord(record);

        Assert.Same(record, patient.MedicalRecord);
        Assert.True(patient.HasMedicalRecord());
    }

    [Fact]
    public void AssignMedicalRecord_WhenPatientAlreadyHasOne_ThrowsInvalidOperationException()
    {
        var patient = NewPatient(medicalRecord: NewMedicalRecord(id: "record-001"));

        Assert.Throws<InvalidOperationException>(() =>
            patient.AssignMedicalRecord(NewMedicalRecord(id: "record-002")));
    }

    [Fact]
    public void AssignMedicalRecord_WhenFails_KeepsOriginalRecord()
    {
        var original = NewMedicalRecord(id: "record-001");
        var patient = NewPatient(medicalRecord: original);

        Assert.Throws<InvalidOperationException>(() =>
            patient.AssignMedicalRecord(NewMedicalRecord(id: "record-002")));

        Assert.Same(original, patient.MedicalRecord);
    }

    [Fact]
    public void HasMedicalRecord_ReflectsWhetherRecordExists()
    {
        var withoutRecord = NewPatient();
        var withRecord = NewPatient(medicalRecord: NewMedicalRecord());

        Assert.False(withoutRecord.HasMedicalRecord());
        Assert.True(withRecord.HasMedicalRecord());
    }

    // ==========================================
    // Consultas de estado
    // ==========================================

    [Fact]
    public void IsActive_ForNewPatient_ReturnsTrue()
    {
        var patient = NewPatient();

        Assert.True(patient.IsActive());
        Assert.False(patient.IsDischarged());
    }

    [Fact]
    public void IsDischarged_AfterDischarge_ReturnsTrue()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        patient.Discharge("nurse-001");

        Assert.True(patient.IsDischarged());
        Assert.False(patient.IsActive());
    }

    [Fact]
    public void IsAssignedToNurse_ReflectsNurseAssignment()
    {
        var patient = NewPatient();
        Assert.False(patient.IsAssignedToNurse());

        patient.AssignNurse("nurse-001", "facility-001");
        Assert.True(patient.IsAssignedToNurse());

        patient.Discharge("nurse-001");
        Assert.False(patient.IsAssignedToNurse());
    }

    // ==========================================
    // ToPrimitives
    // ==========================================

    [Fact]
    public void ToPrimitives_ReturnsAllFieldsCorrectly()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");

        var primitives = patient.ToPrimitives();

        Assert.Equal(patient.Id, primitives.Id);
        Assert.Equal(patient.Name, primitives.Name);
        Assert.Equal(patient.LastName, primitives.LastName);
        Assert.Equal(patient.BirthDate.Value, primitives.BirthDate);
        Assert.Equal(patient.CurrentWeight.Value, primitives.CurrentWeight);
        Assert.Equal(patient.CurrentHeight.Value, primitives.CurrentHeight);
        Assert.Equal(patient.MotherId, primitives.MotherId);
        Assert.Equal("nurse-001", primitives.NurseId);
        Assert.Equal(patient.Gender.ToStringValue(), primitives.Gender);
        Assert.Equal("facility-001", primitives.FacilityId);
        Assert.Equal(patient.Status.ToStringValue(), primitives.Status);
    }

    [Fact]
    public void ToPrimitives_WithoutMedicalRecord_ReturnsNullMedicalRecord()
    {
        var patient = NewPatient();

        var primitives = patient.ToPrimitives();

        Assert.Null(primitives.MedicalRecord);
        Assert.Null(primitives.NurseId);
        Assert.Null(primitives.FacilityId);
    }

    [Fact]
    public void ToPrimitives_WithMedicalRecord_IncludesItsPrimitives()
    {
        var patient = NewPatient(medicalRecord: NewMedicalRecord(id: "record-001"));

        var primitives = patient.ToPrimitives();

        Assert.NotNull(primitives.MedicalRecord);
        Assert.Equal("record-001", primitives.MedicalRecord!.Id);
    }

    [Fact]
    public void ToPrimitives_AfterDischarge_ReflectsDischargedStatus()
    {
        var patient = NewPatient(nurseId: "nurse-001", facilityId: "facility-001");
        patient.Discharge("nurse-001");

        var primitives = patient.ToPrimitives();

        Assert.Equal(PatientStatus.Discharged.ToStringValue(), primitives.Status);
        Assert.Null(primitives.NurseId);
        Assert.Null(primitives.FacilityId);
    }
}
