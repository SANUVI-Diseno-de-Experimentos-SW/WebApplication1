using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

/// <summary>
/// Prepara datos reales a traves de la propia API: una madre con un paciente registrado y,
/// opcionalmente, asignado a una enfermera, con historia clinica y controles de hemoglobina.
/// Cada escenario usa usuarios nuevos, asi que los tests no se afectan entre si.
/// </summary>
public sealed class PatientScenario
{
    private readonly MongoWebApplicationFactory _factory;
 
    private PatientScenario(MongoWebApplicationFactory factory, HttpClient motherClient,
        AuthSession mother, string motherId, string patientId)
    {
        _factory = factory;
        MotherClient = motherClient;
        Mother = mother;
        MotherId = motherId;
        PatientId = patientId;
    }
 
    public HttpClient MotherClient { get; }
    public AuthSession Mother { get; }
    public string MotherId { get; }
    public string PatientId { get; }
 
    public HttpClient? NurseClient { get; private set; }
    public AuthSession? Nurse { get; private set; }
 
    /// <summary>Madre nueva + un paciente registrado (todavia sin enfermera).</summary>
    public static async Task<PatientScenario> CreateAsync(MongoWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var session = await IamAuth.CreateMotherSessionAsync(client);
        client.UseToken(session.Token);
 
        var created = await TestApi.PostAsync(client, PatientManagementApi.RegisterUrl, PatientManagementApi.NewPatient());
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);
 
        // El registro solo devuelve un mensaje, asi que el id se lee desde "mis pacientes".
        var response = await client.GetAsync(PatientManagementApi.MyPatientsUrl);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var motherId = json.FindString("motherId");
        var patients = json.Find("patients");
        var patientId = patients is { ValueKind: JsonValueKind.Array } list && list.GetArrayLength() > 0
            ? list[0].FindString("patientId", "id")
            : null;
 
        if (string.IsNullOrEmpty(motherId) || string.IsNullOrEmpty(patientId))
            Assert.Fail($"No pude leer motherId/patientId de GET my-patients. Respuesta: {json}");
 
        return new PatientScenario(factory, client, session, motherId!, patientId!);
    }
 
    /// <summary>
    /// Crea un cliente autenticado como una enfermera nueva. Por defecto queda asignada a una posta
    /// de salud, requisito de la API para poder recibir pacientes.
    /// </summary>
    public static async Task<(HttpClient Client, AuthSession Session)> CreateNurseAsync(
        MongoWebApplicationFactory factory, bool withFacility = true)
    {
        var client = factory.CreateClient();
        var session = await IamAuth.CreateStaffSessionAsync(client, "Nurse");
        client.UseToken(session.Token);
 
        if (withFacility)
            await HealthFacilitySetup.AssignNurseToNewFacilityAsync(factory, session.Token);
 
        return (client, session);
    }
    
    /// <summary>Crea una enfermera y le asigna el paciente.</summary>
    public async Task AssignNurseAsync()
    {
        var (client, session) = await CreateNurseAsync(_factory);
        NurseClient = client;
        Nurse = session;
 
        var response = await TestApi.PostAsync(client, PatientManagementApi.AssignNurseUrl, new { PatientId });
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }
 
    public async Task CreateMedicalRecordAsync()
    {
        var response = await TestApi.PostAsync(NurseClient!, PatientManagementApi.MedicalRecordUrl,
            PatientManagementApi.NewMedicalRecord(PatientId));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }

 
    public async Task<string> GetMedicalRecordIdAsync()
    {
        var response = await NurseClient!.GetAsync(PatientManagementApi.MedicalRecordOfUrl(PatientId));
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.FindString("id", "medicalRecordId");
        if (string.IsNullOrEmpty(id))
            Assert.Fail($"No pude leer el id de la historia clinica. Respuesta: {json}");
 
        return id!;
    }
 
    public async Task RegisterHemoglobinAsync(double level = 11.5)
    {
        var response = await TestApi.PostAsync(NurseClient!, PatientManagementApi.HemoglobinControlUrl,
            new { PatientId, HemoglobinLevel = level });
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }
}