using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

// ==========================================
// POST register              GET my-patients
// POST assign-nurse          GET mother/{motherId}
// GET  nurse                 GET {id}
// GET  mother/search/{term}
// ==========================================
[Collection(ApiCollection.Name)]
public class PatientRegistrationAndAssignmentTests
{
    private readonly MongoWebApplicationFactory _factory;
 
    public PatientRegistrationAndAssignmentTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }
 
    private async Task<HttpClient> NewMotherClientAsync()
    {
        var client = _factory.CreateClient();
        client.UseToken(await IamAuth.GetMotherTokenAsync(client));
        return client;
    }
 
    // ---------- POST register ----------
 
    [Fact]
    public async Task RegisterPatient_ValidData_Returns201()
    {
        var mother = await NewMotherClientAsync();
 
        var response = await TestApi.PostAsync(mother, PatientManagementApi.RegisterUrl, PatientManagementApi.NewPatient());
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Patient registered successfully", json.FindString("message"));
    }
 
    // Si el MotherId del body se usara en lugar del token, el paciente no aparecería en "mis pacientes".
    [Fact]
    public async Task RegisterPatient_UsesMotherIdFromToken_AndAppearsInMyPatients()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
 
        var response = await scenario.MotherClient.GetAsync(PatientManagementApi.MyPatientsUrl);
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains(scenario.PatientId, body);
        Assert.DoesNotContain("se-reemplaza-por-el-del-token", body);
    }
 
    // ADVERTENCIA: asume que el dominio rechaza un peso negativo. Si recibes 201,
    // falta esa validacion en el dominio.
    [Fact]
    public async Task RegisterPatient_NegativeWeight_Returns400()
    {
        var mother = await NewMotherClientAsync();
 
        var response = await TestApi.PostAsync(mother, PatientManagementApi.RegisterUrl,
            PatientManagementApi.NewPatient(weight: -1));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- GET my-patients ----------
 
    [Fact]
    public async Task MyPatients_MotherWithoutPatients_ReturnsEmptyList()
    {
        var mother = await NewMotherClientAsync();
 
        var response = await mother.GetAsync(PatientManagementApi.MyPatientsUrl);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, json.Find("patients")!.Value.GetArrayLength());
    }
 
    // ---------- GET mother/{motherId} (enfermera) ----------
 
    [Fact]
    public async Task ListByMother_BeforeAssignment_ShowsUnassigned()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.GetAsync(PatientManagementApi.ByMotherUrl(scenario.MotherId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = json.Find("data")!.Value.EnumerateArray().First();
        Assert.Equal(scenario.PatientId, first.FindString("patientId"));
        Assert.Equal("UNASSIGNED", first.FindString("statusAssignment"));
    }
 
    [Fact]
    public async Task ListByMother_AfterAssignment_ShowsAssigned()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        await scenario.AssignNurseAsync();
 
        var response = await scenario.NurseClient!.GetAsync(PatientManagementApi.ByMotherUrl(scenario.MotherId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var first = json.Find("data")!.Value.EnumerateArray().First();
        Assert.Equal("ASSIGNED", first.FindString("statusAssignment"));
    }
 
    // ---------- POST assign-nurse ----------
 
    [Fact]
    public async Task AssignNurse_ValidPatient_Returns201()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await TestApi.PostAsync(nurse, PatientManagementApi.AssignNurseUrl,
            new { PatientId = scenario.PatientId });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Patient assigned successfully", json.FindString("message"));
    }
 
    [Fact]
    public async Task AssignNurse_MissingPatientId_Returns400()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await TestApi.PostAsync(nurse, PatientManagementApi.AssignNurseUrl, new { PatientId = "" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ADVERTENCIA: asume que asignar un paciente inexistente lanza una excepcion (-> 400).
    [Fact]
    public async Task AssignNurse_NonExistentPatient_Returns400()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await TestApi.PostAsync(nurse, PatientManagementApi.AssignNurseUrl,
            new { PatientId = "000000000000000000000000" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- GET nurse (pacientes asignados) ----------
 
    [Fact]
    public async Task NursePatients_WithoutAssignments_ReturnsEmpty()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.GetAsync(PatientManagementApi.NurseUrl);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("EMPTY", json.FindString("status"));
        Assert.Equal("0", json.Find("data")!.Value.FindString("total"));
    }
 
    [Fact]
    public async Task NursePatients_AfterAssignment_ReturnsThePatient()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        await scenario.AssignNurseAsync();
 
        var response = await scenario.NurseClient!.GetAsync(PatientManagementApi.NurseUrl);
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("SUCCESS", json.FindString("status"));
        Assert.Equal("1", json.Find("data")!.Value.FindString("total"));
        Assert.Contains(scenario.PatientId, body);
    }
 
    // ---------- GET {id} (informacion basica) ----------
 
    [Fact]
    public async Task GetPatientBasicInfo_ExistingPatient_Returns200()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.GetAsync(PatientManagementApi.PatientUrl(scenario.PatientId));
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains(scenario.PatientId, body);
    }
 
    // El controller devuelve 404 si el servicio retorna null, o 400 si lanza una excepcion.
    [Fact]
    public async Task GetPatientBasicInfo_NonExistentPatient_Returns404Or400()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.GetAsync(PatientManagementApi.PatientUrl("000000000000000000000000"));
 
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.BadRequest });
    }
 
    // ---------- GET mother/search/{term} ----------
 
    [Fact]
    public async Task SearchMother_ByExistingDni_ReturnsTheMother()
    {
        var scenario = await PatientScenario.CreateAsync(_factory);
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
 
        var response = await nurse.GetAsync(PatientManagementApi.SearchMotherUrl(scenario.Mother.Dni));
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains(scenario.Mother.Dni, body);
    }
}