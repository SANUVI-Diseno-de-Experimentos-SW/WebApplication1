using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

/// <summary>
/// Prepara un paciente listo para iniciar un tratamiento: asignado a una enfermera, con historia
/// clinica y un control de hemoglobina bajo (9.5). Con CreateWithTreatmentAsync ademas se inicia el tratamiento.
/// </summary>
public sealed class TreatmentScenario
{
    private TreatmentScenario(PatientScenario patient)
    {
        Patient = patient;
    }

    public PatientScenario Patient { get; }
    public HttpClient NurseClient => Patient.NurseClient!;
    public HttpClient MotherClient => Patient.MotherClient;
    public string PatientId => Patient.PatientId;

    public static async Task<TreatmentScenario> CreateAssignedAsync(MongoWebApplicationFactory factory)
    {
        var patient = await PatientScenario.CreateAsync(factory);
        await patient.AssignNurseAsync();
        await patient.CreateMedicalRecordAsync();
        await patient.RegisterHemoglobinAsync(9.5);
        return new TreatmentScenario(patient);
    }

    public static async Task<TreatmentScenario> CreateWithTreatmentAsync(MongoWebApplicationFactory factory)
    {
        var scenario = await CreateAssignedAsync(factory);
        await scenario.StartAsync();
        return scenario;
    }

    /// <summary>Envia "iniciar tratamiento" sin verificar la respuesta.</summary>
    public Task<HttpResponseMessage> StartRawAsync(HttpClient? nurse = null, string? patientId = null) =>
        TestApi.PostAsync(nurse ?? NurseClient, TreatmentTrackingApi.TreatmentsUrl,
            TreatmentTrackingApi.NewTreatment(patientId ?? PatientId));

    public async Task StartAsync()
    {
        var response = await StartRawAsync();
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }

    /// <summary>Tratamientos de la enfermera (opcionalmente filtrados por estado).</summary>
    public async Task<JsonElement> GetNurseTreatmentsAsync(string? status = null, HttpClient? nurse = null)
    {
        var url = status is null
            ? TreatmentTrackingApi.NurseTreatmentsUrl
            : $"{TreatmentTrackingApi.NurseTreatmentsUrl}?status={Uri.EscapeDataString(status)}";

        var response = await (nurse ?? NurseClient).GetAsync(url);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return ItemsOf(json);
    }

    /// <summary>"Iniciar tratamiento" puede no devolver el id, asi que se lee de la lista de la enfermera.</summary>
    public async Task<string> GetTreatmentIdAsync()
    {
        var items = await GetNurseTreatmentsAsync();
        var id = items is { ValueKind: JsonValueKind.Array } && items.GetArrayLength() > 0
            ? items[0].FindString("treatmentId", "id")
            : null;

        if (string.IsNullOrEmpty(id))
            Assert.Fail($"No pude leer el id del tratamiento desde GET nurses/treatments. Respuesta: {items}");

        return id!;
    }

    /// <summary>La lista puede venir directa o dentro de una propiedad (treatments, data...).</summary>
    public static JsonElement ItemsOf(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Array) return json;
        return json.Find("treatments", "data", "items", "results") ?? json;
    }
}