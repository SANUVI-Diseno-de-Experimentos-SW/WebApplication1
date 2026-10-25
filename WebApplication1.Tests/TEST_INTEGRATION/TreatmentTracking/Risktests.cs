using System.Net;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

// ==========================================
// GET risk-overview               (Nurse) resumen por nivel de riesgo
// GET risk/{riskLevel}/patients   (Nurse) pacientes de un nivel de riesgo
// ==========================================
[Collection(ApiCollection.Name)]
public class RiskTests
{
    private readonly MongoWebApplicationFactory _factory;

    public RiskTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ---------- GET risk-overview ----------

    [Fact]
    public async Task RiskOverview_NurseWithoutPatients_Returns200()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await nurse.GetAsync(TreatmentTrackingApi.RiskOverviewUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    [Fact]
    public async Task RiskOverview_NurseWithTreatment_Returns200WithContent()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await scenario.NurseClient.GetAsync(TreatmentTrackingApi.RiskOverviewUrl);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    // ---------- GET risk/{riskLevel}/patients ----------

    // ADVERTENCIA: "HIGH" es un nombre supuesto; ajusta ValidRiskLevel en TreatmentTrackingApi.cs
    // a los valores de tu enum RiskLevel.
    [Fact]
    public async Task PatientsByRiskLevel_ValidLevel_Returns200()
    {
        var scenario = await TreatmentScenario.CreateWithTreatmentAsync(_factory);

        var response = await scenario.NurseClient.GetAsync(
            TreatmentTrackingApi.PatientsByRiskUrl(TreatmentTrackingApi.ValidRiskLevel));

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    [Fact]
    public async Task PatientsByRiskLevel_InvalidLevel_Returns400()
    {
        var (nurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await nurse.GetAsync(TreatmentTrackingApi.PatientsByRiskUrl("NIVEL-INEXISTENTE"));

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
}