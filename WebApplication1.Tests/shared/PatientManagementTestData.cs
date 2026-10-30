using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Enums;
using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.Tests.PatientManagement;

/// <summary>
/// Fabrica compartida de objetos para las pruebas de Patient, MedicalRecord y Control.
///
/// IMPORTANTE: los constructores de los value objects (BirthDate, Weight, Height,
/// HemoglobinLevel, MotivoConsulta, Observaciones, Antecedente) estan SUPUESTOS.
/// Si alguno se crea distinto (por ejemplo Weight.Create(...)), ajustalo SOLO aqui:
/// las pruebas de los tres archivos usan estos metodos y no cambian.
/// </summary>
internal static class PatientManagementTestData
{
    // ==========================================
    // VALUE OBJECTS (ajustar aqui si no compilan)
    // ==========================================

    public static BirthDate NewBirthDate() => new(new DateTime(2025, 1, 15));

    public static Weight NewWeight(double value = 8.5) => new(value);

    public static Height NewHeight(double value = 70) => new(value);

    public static HemoglobinLevel NewHemoglobin(double? value = 12.0) => new(value);

    public static MotivoConsulta NewMotivoConsulta(string value = "Control de anemia") => new(value);

    public static Observaciones NewObservaciones(string value = "Sin observaciones") => new(value);

    public static Antecedente NewAntecedente(string type) => new(type, "Descripcion");

    // Primer valor del enum: no depende de nombres concretos de Gender.
    public static Gender AnyGender() => Enum.GetValues<Gender>().First();

    // ==========================================
    // FECHAS
    // ==========================================

    public static DateTime PastDate() => DateTime.UtcNow.AddDays(-1);

    // ==========================================
    // ENTIDADES / AGREGADOS
    // ==========================================

    public static Control NewControl(
        string id = "control-001",
        double? hemoglobin = 12.0,
        DateTime? date = null)
    {
        return new Control(id, date ?? PastDate(), NewHemoglobin(hemoglobin));
    }

    public static MedicalRecord NewMedicalRecord(
        string id = "record-001",
        DateTime? createdAt = null,
        string patientId = "patient-001",
        string? nurseId = null,
        HemoglobinLevel? hemoglobinLevel = null,
        List<Antecedente>? antecedentes = null,
        List<string>? sintomas = null,
        List<Control>? controls = null)
    {
        return new MedicalRecord(
            id,
            createdAt ?? PastDate(),
            NewWeight(),
            NewHeight(),
            AnyGender(),
            NewMotivoConsulta(),
            NewObservaciones(),
            patientId,
            nurseId,
            hemoglobinLevel,
            antecedentes,
            sintomas,
            controls);
    }

    public static Patient NewPatient(
        string id = "patient-001",
        string name = "Ana",
        string lastName = "Perez",
        string motherId = "mother-001",
        string? nurseId = null,
        string? facilityId = null,
        PatientStatus status = PatientStatus.Active,
        MedicalRecord? medicalRecord = null)
    {
        return new Patient(
            id,
            name,
            lastName,
            NewBirthDate(),
            NewWeight(),
            NewHeight(),
            motherId,
            AnyGender(),
            nurseId,
            facilityId,
            status,
            medicalRecord);
    }
}
