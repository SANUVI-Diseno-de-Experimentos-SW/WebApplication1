using System.Net;
using System.Net.Http.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

// ==========================================
// Seguridad de api/health-facilities
//   Solo Admin  : POST (registrar posta), POST assign-nurse
//   Solo Mother : POST appointments, PUT appointments/cancel, GET nearby, GET appointments/mother/next
//   Solo Nurse  : GET appointments/nurse
//   Publico     : GET {id}
// ==========================================
[Collection(ApiCollection.Name)]
public class HealthFacilitySecurityTests
{
    private readonly HttpClient _client;

    public HealthFacilitySecurityTests(MongoWebApplicationFactory factory)
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
    [InlineData("POST", HealthFacilityApi.BaseUrl)]
    [InlineData("POST", HealthFacilityApi.AssignNurseUrl)]
    [InlineData("POST", HealthFacilityApi.AppointmentsUrl)]
    [InlineData("PUT", HealthFacilityApi.CancelAppointmentUrl)]
    [InlineData("GET", HealthFacilityApi.NearbyUrl)]
    [InlineData("GET", HealthFacilityApi.NurseAppointmentsUrl)]
    [InlineData("GET", HealthFacilityApi.MotherNextAppointmentUrl)]
    public async Task Endpoints_WithoutToken_Returns401(string method, string url)
    {
        var response = await SendAsync(method, url);

        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }

    // ---------- 2. Rol incorrecto: 403 ----------
    // ADVERTENCIA: asume que [RequireRole] responde 403 cuando el rol no coincide.

    [Theory]
    [InlineData("Mother", "POST", HealthFacilityApi.BaseUrl)]
    [InlineData("Nurse", "POST", HealthFacilityApi.BaseUrl)]
    [InlineData("Mother", "POST", HealthFacilityApi.AssignNurseUrl)]
    [InlineData("Nurse", "POST", HealthFacilityApi.AssignNurseUrl)]
    [InlineData("Nurse", "POST", HealthFacilityApi.AppointmentsUrl)]
    [InlineData("Nurse", "PUT", HealthFacilityApi.CancelAppointmentUrl)]
    [InlineData("Nurse", "GET", HealthFacilityApi.NearbyUrl)]
    [InlineData("Nurse", "GET", HealthFacilityApi.MotherNextAppointmentUrl)]
    [InlineData("Mother", "GET", HealthFacilityApi.NurseAppointmentsUrl)]
    public async Task Endpoints_WithWrongRole_Returns403(string role, string method, string url)
    {
        var token = role == "Mother"
            ? await IamAuth.GetMotherTokenAsync(_client)
            : await IamAuth.GetStaffTokenAsync(_client, role);
        _client.UseToken(token);

        var response = await SendAsync(method, url);

        await TestApi.AssertStatusAsync(HttpStatusCode.Forbidden, response);
    }

    // ---------- 3. Los tokens traen los claims necesarios ----------

    [Fact]
    public async Task NurseToken_IncludesNurseIdClaim()
    {
        var token = await IamAuth.GetStaffTokenAsync(_client, "Nurse");

        Assert.False(string.IsNullOrEmpty(JwtClaims.Get(token, "nurseId")));
    }
}