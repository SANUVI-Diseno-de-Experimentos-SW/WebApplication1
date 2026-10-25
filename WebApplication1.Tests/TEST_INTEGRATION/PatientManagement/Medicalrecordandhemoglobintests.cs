using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;

namespace WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

// ==========================================
// POST medical-record          GET {patientId}/medical-record
// POST hemoglobin-control      GET medical-record/{id}/controls
// GET  {patientId}/hemoglobin-evolution (madre)
// PUT  discharge
// ==========================================
[Collection(ApiCollection.Name)]
public class MedicalRecordAndHemoglobinTests
{
    private readonly MongoWebApplicationFactory _factory;
 
    public MedicalRecordAndHemoglobinTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }
 
    // Paciente asignado a una enfermera, listo para crear la historia clinica
    private async Task<PatientScenario> AssignedPatientAsync()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        await scenario.AssignNurseAsync();
        return scenario;
    }
 
    // ---------- POST medical-record ----------
 
    [Fact]
    public async Task CreateMedicalRecord_AssignedPatient_Returns201()
    {
        var scenario = await AssignedPatientAsync();
 
        var response = await TestApi.PostAsync(scenario.NurseClient!, PatientManagementApi.MedicalRecordUrl,
            PatientManagementApi.NewMedicalRecord(scenario.PatientId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Medical record created successfully", json.FindString("message"));
    }
 
    // Un paciente asignado a OTRA enfermera no se puede tocar
    [Fact]
    public async Task CreateMedicalRecord_PatientAssignedToAnotherNurse_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await TestApi.PostAsync(otherNurse, PatientManagementApi.MedicalRecordUrl,
            PatientManagementApi.NewMedicalRecord(scenario.PatientId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task CreateMedicalRecord_MissingRequiredFields_Returns400()
    {
        var scenario = await AssignedPatientAsync();
 
        var response = await TestApi.PostAsync(scenario.NurseClient!, PatientManagementApi.MedicalRecordUrl,
            new { PatientId = scenario.PatientId, Weight = 8.5, Height = 70.0, MotivoConsulta = "", Observaciones = "" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- GET {patientId}/medical-record ----------
 
    [Fact]
    public async Task GetMedicalRecord_AfterCreation_ReturnsSavedData()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
 
        var response = await scenario.NurseClient!.GetAsync(PatientManagementApi.MedicalRecordOfUrl(scenario.PatientId));
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains("Control de rutina", body);
        Assert.Contains("Sin novedades", body);
    }
 
    // Sin historia clinica: 404 si el servicio devuelve null, o 400 si lanza excepcion.
    [Fact]
    public async Task GetMedicalRecord_BeforeCreation_Returns404Or400()
    {
        var scenario = await AssignedPatientAsync();
 
        var response = await scenario.NurseClient!.GetAsync(PatientManagementApi.MedicalRecordOfUrl(scenario.PatientId));
 
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.BadRequest });
    }
 
    [Fact]
    public async Task GetMedicalRecord_PatientAssignedToAnotherNurse_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await otherNurse.GetAsync(PatientManagementApi.MedicalRecordOfUrl(scenario.PatientId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- POST hemoglobin-control ----------
 
    [Fact]
    public async Task RegisterHemoglobin_ValidLevel_Returns201()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
 
        var response = await TestApi.PostAsync(scenario.NurseClient!, PatientManagementApi.HemoglobinControlUrl,
            new { PatientId = scenario.PatientId, HemoglobinLevel = 11.5 });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Hemoglobin control registered successfully", json.FindString("message"));
    }
 
    [Fact]
    public async Task RegisterHemoglobin_MissingLevel_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
 
        var response = await TestApi.PostAsync(scenario.NurseClient!, PatientManagementApi.HemoglobinControlUrl,
            new { PatientId = scenario.PatientId });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task RegisterHemoglobin_PatientAssignedToAnotherNurse_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await TestApi.PostAsync(otherNurse, PatientManagementApi.HemoglobinControlUrl,
            new { PatientId = scenario.PatientId, HemoglobinLevel = 11.5 });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- GET medical-record/{id}/controls ----------
 
    [Fact]
    public async Task GetControls_AfterRegisteringHemoglobin_ReturnsTheControl()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
        await scenario.RegisterHemoglobinAsync(11.5);
        var medicalRecordId = await scenario.GetMedicalRecordIdAsync();
 
        var response = await scenario.NurseClient!.GetAsync(PatientManagementApi.ControlsUrl(medicalRecordId));
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains("11.5", body);
    }
 
    [Fact]
    public async Task GetControls_NurseWithoutAccess_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
        var medicalRecordId = await scenario.GetMedicalRecordIdAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await otherNurse.GetAsync(PatientManagementApi.ControlsUrl(medicalRecordId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- GET {patientId}/hemoglobin-evolution (madre) ----------
 
    [Fact]
    public async Task HemoglobinEvolution_OwnPatient_Returns200()
    {
        var scenario = await AssignedPatientAsync();
        await scenario.CreateMedicalRecordAsync();
        await scenario.RegisterHemoglobinAsync(11.5);
 
        var response = await scenario.MotherClient.GetAsync(PatientManagementApi.HemoglobinEvolutionUrl(scenario.PatientId));
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains("11.5", body);
    }
 
    [Fact]
    public async Task HemoglobinEvolution_PatientOfAnotherMother_Returns400()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        var other = await PatientScenario.CreateAsync(_factory);
 
        var response = await other.MotherClient.GetAsync(PatientManagementApi.HemoglobinEvolutionUrl(scenario.PatientId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- PUT discharge ----------
    // Solo se prueban los errores: el camino feliz depende de las reglas de alta de tu dominio
    // (por ejemplo, cuantos controles o que nivel de hemoglobina se exige).
 
    [Fact]
    public async Task Discharge_MissingPatientId_Returns400()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.PutAsJsonAsync(PatientManagementApi.DischargeUrl, new { PatientId = "" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task Discharge_PatientAssignedToAnotherNurse_Returns400()
    {
        var scenario = await AssignedPatientAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await otherNurse.PutAsJsonAsync(PatientManagementApi.DischargeUrl,
            new { PatientId = scenario.PatientId });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
}
 