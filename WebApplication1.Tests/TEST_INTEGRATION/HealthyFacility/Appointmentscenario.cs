using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

/// <summary>
/// Prepara lo necesario para probar citas: una madre con un paciente, y una enfermera
/// asignada a su propia posta. Las citas se reservan en la posta de esa enfermera.
/// </summary>
public sealed class AppointmentScenario
{
    private AppointmentScenario(PatientScenario patient, HttpClient nurseClient, string facilityId)
    {
        Patient = patient;
        NurseClient = nurseClient;
        FacilityId = facilityId;
    }

    public PatientScenario Patient { get; }
    public HttpClient NurseClient { get; }
    public string FacilityId { get; }

    public string Date { get; } = HealthFacilityApi.NextWeekdayDate();
    public string Time { get; } = HealthFacilityApi.ValidTime;

    public static async Task<AppointmentScenario> CreateAsync(MongoWebApplicationFactory factory)
    {
        var patient = await PatientScenario.CreateAsync(factory);

        var nurseClient = factory.CreateClient();
        var session = await IamAuth.CreateStaffSessionAsync(nurseClient, "Nurse");
        nurseClient.UseToken(session.Token);
        var facilityId = await HealthFacilitySetup.AssignNurseToNewFacilityAsync(factory, session.Token);

        return new AppointmentScenario(patient, nurseClient, facilityId);
    }

    /// <summary>Envia una reserva sin verificar la respuesta. Por defecto: la madre y el paciente del escenario.</summary>
    public Task<HttpResponseMessage> BookRawAsync(HttpClient? motherClient = null, string? patientId = null,
        string? facilityId = null, string? date = null, string? time = null) =>
        TestApi.PostAsync(motherClient ?? Patient.MotherClient, HealthFacilityApi.AppointmentsUrl,
            HealthFacilityApi.NewAppointment(facilityId ?? FacilityId, patientId ?? Patient.PatientId,
                date ?? Date, time ?? Time));

    /// <summary>Reserva la cita y verifica 201.</summary>
    public async Task BookAsync()
    {
        var response = await BookRawAsync();
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }

    /// <summary>Historial de citas del paciente (lo ve la madre).</summary>
    public async Task<JsonElement> GetHistoryAsync()
    {
        var response = await Patient.MotherClient.GetAsync(HealthFacilityApi.PatientAppointmentsUrl(Patient.PatientId));
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    /// <summary>La reserva solo devuelve un mensaje, asi que el id de la cita se lee del historial.</summary>
    public async Task<string> GetAppointmentIdAsync()
    {
        var history = await GetHistoryAsync();
        var id = history is { ValueKind: JsonValueKind.Array } && history.GetArrayLength() > 0
            ? history[0].FindString("appointmentId", "id")
            : null;

        if (string.IsNullOrEmpty(id))
            Assert.Fail($"No pude leer el id de la cita desde el historial. Respuesta: {history}");

        return id!;
    }
}