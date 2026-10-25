using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

// ==========================================
// GET  patients/{id}/today-dose    (Mother) dosis de hoy
// POST doses/confirm               (Mother) confirmar dosis
// GET  patients/{id}/dose-history  (Mother) historial de dosis
// ==========================================
[Collection(ApiCollection.Name)]
public class DoseTests
{
    private readonly MongoWebApplicationFactory _factory;

    public DoseTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ---------- GET today-dose ----------

    [Fact]
    public async Task TodayDose_OwnPatientWithTreatment_Returns200()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await scenario.MotherClient.GetAsync(TreatmentTrackingApi.TodayDoseUrl(scenario.PatientId));

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    // ADVERTENCIA: asume que GetTodayDoseQuery valida que el paciente sea de la madre.
    [Fact]
    public async Task TodayDose_PatientOfAnotherMother_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await otherMother.MotherClient.GetAsync(TreatmentTrackingApi.TodayDoseUrl(scenario.PatientId));

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- POST doses/confirm ----------

    // ADVERTENCIA: depende de la hora. Si tu API solo permite confirmar dentro de una ventana
    // cercana al horario de la dosis, este test puede fallar segun la hora a la que corra;
    // el mensaje del error lo indicara.
    [Fact]
    public async Task ConfirmDose_OwnPatient_Returns200()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await TestApi.PostAsync(scenario.MotherClient, TreatmentTrackingApi.ConfirmDoseUrl,
            new { PatientId = scenario.PatientId });

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    // ADVERTENCIA: asume que confirmar valida que el paciente sea de la madre. Si recibes 200,
    // cualquier madre puede confirmar dosis de pacientes ajenos (fallo de autorizacion).
    [Fact]
    public async Task ConfirmDose_PatientOfAnotherMother_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await TestApi.PostAsync(otherMother.MotherClient, TreatmentTrackingApi.ConfirmDoseUrl,
            new { PatientId = scenario.PatientId });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact]
    public async Task ConfirmDose_PatientWithoutTreatment_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory); // sin tratamiento

        var response = await TestApi.PostAsync(scenario.MotherClient, TreatmentTrackingApi.ConfirmDoseUrl,
            new { PatientId = scenario.PatientId });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact]
    public async Task ConfirmDose_NonExistentPatient_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await TestApi.PostAsync(scenario.MotherClient, TreatmentTrackingApi.ConfirmDoseUrl,
            new { PatientId = "000000000000000000000000" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- GET dose-history ----------

    [Fact]
    public async Task DoseHistory_OwnPatientWithTreatment_Returns200()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await scenario.MotherClient.GetAsync(TreatmentTrackingApi.DoseHistoryUrl(scenario.PatientId));

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    // ADVERTENCIA: asume que, despues de confirmar, el historial muestra la dosis como confirmada
    // (texto "CONFIRM" en su estado). Ajusta el texto si tu estado se llama distinto.
    [Fact]
    public async Task DoseHistory_AfterConfirming_ShowsConfirmedDose()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var confirm = await TestApi.PostAsync(scenario.MotherClient, TreatmentTrackingApi.ConfirmDoseUrl,
            new { PatientId = scenario.PatientId });
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, confirm);

        var response = await scenario.MotherClient.GetAsync(TreatmentTrackingApi.DoseHistoryUrl(scenario.PatientId));
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains("CONFIRM", body, StringComparison.OrdinalIgnoreCase);
    }

    // OJO: en el controller, dose-history obtiene el motherId del token pero NO lo usa en la consulta
    // (GetPatientDoseHistoryQuery solo recibe patientId). Este test espera 400 al consultar el
    // historial de un paciente ajeno. Si recibes 200, es una fuga de datos reales.
    [Fact]
    public async Task DoseHistory_PatientOfAnotherMother_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await otherMother.MotherClient.GetAsync(TreatmentTrackingApi.DoseHistoryUrl(scenario.PatientId));

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
}