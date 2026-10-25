namespace WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

/// <summary>URLs y datos de prueba del contexto PatientManagement (api/patients).</summary>
public static class PatientManagementApi
{
    public const string BaseUrl = "/api/patients";
 
    public const string RegisterUrl = BaseUrl + "/register";
    public const string AssignNurseUrl = BaseUrl + "/assign-nurse";
    public const string MedicalRecordUrl = BaseUrl + "/medical-record";
    public const string HemoglobinControlUrl = BaseUrl + "/hemoglobin-control";
    public const string DischargeUrl = BaseUrl + "/discharge";
    public const string MyPatientsUrl = BaseUrl + "/my-patients";
    public const string NurseUrl = BaseUrl + "/nurse";
 
    public static string ByMotherUrl(string motherId) => $"{BaseUrl}/mother/{motherId}";
    public static string SearchMotherUrl(string term) => $"{BaseUrl}/mother/search/{Uri.EscapeDataString(term)}";
    public static string PatientUrl(string id) => $"{BaseUrl}/{id}";
    public static string MedicalRecordOfUrl(string patientId) => $"{BaseUrl}/{patientId}/medical-record";
    public static string ControlsUrl(string medicalRecordId) => $"{BaseUrl}/medical-record/{medicalRecordId}/controls";
    public static string HemoglobinEvolutionUrl(string patientId) => $"{BaseUrl}/{patientId}/hemoglobin-evolution";
 
    // ADVERTENCIA: ajusta este valor al formato que acepte tu dominio para Gender ("Female", "F", etc.)
    public const string ValidGender = "FEMALE";
 
    /// <summary>
    /// MotherId se envia porque el comando lo exige al deserializar, pero el controller
    /// lo reemplaza por el del token (eso tambien se verifica en los tests).
    /// </summary>
    public static object NewPatient(double weight = 8.5, double height = 70.0) => new
    {
        Name = "Baby",
        LastName = "Doe",
        BirthDate = DateTime.UtcNow.AddMonths(-8),
        Gender = ValidGender,
        Weight = weight,
        Height = height,
        MotherId = "se-reemplaza-por-el-del-token"
    };
 
    public static object NewMedicalRecord(string patientId) => new
    {
        PatientId = patientId,
        Weight = 8.5,
        Height = 70.0,
        MotivoConsulta = "Control de rutina",
        Observaciones = "Sin novedades",
        Sintomas = new[] { "fatiga" }
    };
}