using System.Net;
using System.Net.Http.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.TreatmentTracking;

// ==========================================
// Seguridad de api/treatment-tracking
//   Solo Nurse  : POST treatments, PUT treatments/complete, PUT treatments/abandon,
//                 GET risk-overview, GET nurses/treatments, GET risk/{level}/patients
//   Solo Mother : POST doses/confirm, GET patients/{id}/today-dose, GET patients/{id}/dose-history
// ==========================================
[Collection(ApiCollection.Name)]
public class TreatmentSecurityTests
{
    private readonly HttpClient _client;

    public TreatmentSecurityTests(MongoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private Task<HttpResponseMessage> SendAsync(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        return _client.SendAsync(request);
    }

    // ---------- 1. Sin token: 401 ----------

    [Theory]
    [InlineData("POST", TreatmentTrackingApi.TreatmentsUrl)]
    [InlineData("POST", TreatmentTrackingApi.ConfirmDoseUrl)]
    [InlineData("PUT", TreatmentTrackingApi.CompleteTreatmentUrl)]
    [InlineData("PUT", TreatmentTrackingApi.AbandonTreatmentUrl)]
    [InlineData("GET", TreatmentTrackingApi.BaseUrl + "/patients/some-id/today-dose")]
    [InlineData("GET", TreatmentTrackingApi.BaseUrl + "/patients/some-id/dose-history")]
    [InlineData("GET", TreatmentTrackingApi.RiskOverviewUrl)]
    [InlineData("GET", TreatmentTrackingApi.NurseTreatmentsUrl)]
    [InlineData("GET", TreatmentTrackingApi.BaseUrl + "/risk/HIGH/patients")]
    public async Task Endpoints_WithoutToken_Returns401(string method, string url)
    {
        var response = await SendAsync(method, url);

        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }

    // ---------- 2. Rol incorrecto: 403 ----------
    // ADVERTENCIA: asume que [RequireRole] responde 403 cuando el rol no coincide.

    [Theory]
    [InlineData("Mother", "POST", TreatmentTrackingApi.TreatmentsUrl)]
    [InlineData("Mother", "PUT", TreatmentTrackingApi.CompleteTreatmentUrl)]
    [InlineData("Mother", "PUT", TreatmentTrackingApi.AbandonTreatmentUrl)]
    [InlineData("Mother", "GET", TreatmentTrackingApi.RiskOverviewUrl)]
    [InlineData("Mother", "GET", TreatmentTrackingApi.NurseTreatmentsUrl)]
    [InlineData("Mother", "GET", TreatmentTrackingApi.BaseUrl + "/risk/HIGH/patients")]
    [InlineData("Nurse", "POST", TreatmentTrackingApi.ConfirmDoseUrl)]
    [InlineData("Nurse", "GET", TreatmentTrackingApi.BaseUrl + "/patients/some-id/today-dose")]
    [InlineData("Nurse", "GET", TreatmentTrackingApi.BaseUrl + "/patients/some-id/dose-history")]
    public async Task Endpoints_WithWrongRole_Returns403(string role, string method, string url)
    {
        var token = role == "Mother"
            ? await IamAuth.GetMotherTokenAsync(_client)
            : await IamAuth.GetStaffTokenAsync(_client, role);
        _client.UseToken(token);

        var response = await SendAsync(method, url);

        await TestApi.AssertStatusAsync(HttpStatusCode.Forbidden, response);
    }
}