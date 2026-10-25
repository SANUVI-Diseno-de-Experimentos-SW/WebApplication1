using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

// ==========================================
// POST treatments               (Nurse) iniciar tratamiento
// PUT  treatments/complete      (Nurse) completar
// PUT  treatments/abandon       (Nurse) abandonar
// GET  nurses/treatments        (Nurse) lista, con filtro opcional ?status=
// ==========================================
[Collection(ApiCollection.Name)]
public class TreatmentLifecycleTests
{
    private readonly MongoWebApplicationFactory _factory;

    public TreatmentLifecycleTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ---------- POST treatments ----------

    // ADVERTENCIA: si falla con 400, revisa los formatos de TreatmentTrackingApi.cs
    // (Quantity, DosingHours...). El Body del error muestra que rechazo el dominio.
    [Fact]
    public async Task StartTreatment_AssignedPatient_Returns201()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await scenario.StartRawAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }

    [Fact]
    public async Task StartTreatment_AppearsInNurseTreatments()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var items = await scenario.GetNurseTreatmentsAsync();

        Assert.Equal(1, items.GetArrayLength());
        Assert.Contains(TreatmentTrackingApi.SupplementName, items.ToString());
        Assert.Contains(scenario.PatientId, items.ToString());
    }

    [Fact]
    public async Task StartTreatment_MissingPatientId_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await TestApi.PostAsync(scenario.NurseClient, TreatmentTrackingApi.TreatmentsUrl,
            TreatmentTrackingApi.NewTreatment(""));
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("PatientId es requerido", body);
    }

    [Fact]
    public async Task StartTreatment_PatientAssignedToAnotherNurse_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await scenario.StartRawAsync(nurse: otherNurse);

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact]
    public async Task StartTreatment_NonExistentPatient_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await scenario.StartRawAsync(patientId: "000000000000000000000000");

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ADVERTENCIA: asume que el dominio rechaza una duracion de 0 dias o menos.
    // Si recibes 201, falta esa validacion.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task StartTreatment_InvalidDuration_Returns400(int durationDays)
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await TestApi.PostAsync(scenario.NurseClient, TreatmentTrackingApi.TreatmentsUrl, new
        {
            PatientId = scenario.PatientId,
            SupplementName = TreatmentTrackingApi.SupplementName,
            Quantity = TreatmentTrackingApi.Quantity,
            DosingHours = TreatmentTrackingApi.DosingHours,
            DurationDays = durationDays
        });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ADVERTENCIA: asume que el dominio exige el nombre del suplemento.
    [Fact]
    public async Task StartTreatment_EmptySupplementName_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await TestApi.PostAsync(scenario.NurseClient, TreatmentTrackingApi.TreatmentsUrl, new
        {
            PatientId = scenario.PatientId,
            SupplementName = "",
            Quantity = TreatmentTrackingApi.Quantity,
            DosingHours = TreatmentTrackingApi.DosingHours,
            DurationDays = TreatmentTrackingApi.DurationDays
        });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- GET nurses/treatments ----------

    [Fact]
    public async Task NurseTreatments_WithoutTreatments_Returns200WithEmptyList()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory); // otra enfermera, otro paciente

        var items = await scenario.GetNurseTreatmentsAsync(nurse: nurse);

        Assert.Equal(0, items.GetArrayLength());
    }

    [Fact]
    public async Task NurseTreatments_FilterByActive_IncludesTheTreatment()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var items = await scenario.GetNurseTreatmentsAsync(TreatmentTrackingApi.StatusActive);

        Assert.Contains(TreatmentTrackingApi.SupplementName, items.ToString());
    }

    // ADVERTENCIA: asume que "COMPLETED" es un estado valido distinto de "ACTIVE".
    [Fact]
    public async Task NurseTreatments_FilterByCompleted_ExcludesActiveTreatment()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var items = await scenario.GetNurseTreatmentsAsync(TreatmentTrackingApi.StatusCompleted);

        Assert.DoesNotContain(TreatmentTrackingApi.SupplementName, items.ToString());
    }

    [Fact]
    public async Task NurseTreatments_InvalidStatus_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await scenario.NurseClient.GetAsync(
            $"{TreatmentTrackingApi.NurseTreatmentsUrl}?status=ESTADO-INEXISTENTE");

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- PUT treatments/complete ----------

    [Fact]
    public async Task CompleteTreatment_OwnTreatment_Returns200_AndLeavesTheActiveList()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var treatmentId = await scenario.GetTreatmentIdAsync();

        var response = await scenario.NurseClient.PutAsJsonAsync(TreatmentTrackingApi.CompleteTreatmentUrl,
            new { TreatmentId = treatmentId, Observation = "Tratamiento completado sin novedades" });

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);

        var active = await scenario.GetNurseTreatmentsAsync(TreatmentTrackingApi.StatusActive);
        Assert.DoesNotContain(treatmentId, active.ToString());
    }

    [Fact]
    public async Task CompleteTreatment_NonExistentTreatment_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await scenario.NurseClient.PutAsJsonAsync(TreatmentTrackingApi.CompleteTreatmentUrl,
            new { TreatmentId = "000000000000000000000000", Observation = "x" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ADVERTENCIA: asume que el servicio valida que el tratamiento sea de un paciente de la enfermera.
    // Si recibes 200, cualquier enfermera puede cerrar tratamientos ajenos (fallo de autorizacion).
    [Fact]
    public async Task CompleteTreatment_OfAnotherNurse_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var treatmentId = await scenario.GetTreatmentIdAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await otherNurse.PutAsJsonAsync(TreatmentTrackingApi.CompleteTreatmentUrl,
            new { TreatmentId = treatmentId, Observation = "intento ajeno" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- PUT treatments/abandon ----------

    [Fact]
    public async Task AbandonTreatment_OwnTreatment_Returns200_AndLeavesTheActiveList()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var treatmentId = await scenario.GetTreatmentIdAsync();

        var response = await scenario.NurseClient.PutAsJsonAsync(TreatmentTrackingApi.AbandonTreatmentUrl,
            new { TreatmentId = treatmentId, Observation = "La madre no volvio a los controles" });

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);

        var active = await scenario.GetNurseTreatmentsAsync(TreatmentTrackingApi.StatusActive);
        Assert.DoesNotContain(treatmentId, active.ToString());
    }

    [Fact]
    public async Task AbandonTreatment_NonExistentTreatment_Returns400()
    {
        var scenario = await TreatmentScenario.CreateAssignedAsync(_factory);

        var response = await scenario.NurseClient.PutAsJsonAsync(TreatmentTrackingApi.AbandonTreatmentUrl,
            new { TreatmentId = "000000000000000000000000", Observation = "x" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ADVERTENCIA: misma regla de autorizacion que en completar.
    [Fact]
    public async Task AbandonTreatment_OfAnotherNurse_Returns400()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);
        var treatmentId = await scenario.GetTreatmentIdAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await otherNurse.PutAsJsonAsync(TreatmentTrackingApi.AbandonTreatmentUrl,
            new { TreatmentId = treatmentId, Observation = "intento ajeno" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
}