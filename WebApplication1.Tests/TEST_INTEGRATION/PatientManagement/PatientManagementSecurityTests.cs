using System.Net;
using System.Net.Http.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

// ==========================================
// Seguridad de api/patients: token obligatorio y rol correcto en cada endpoint
//   Solo Mother : register, my-patients, {id}/hemoglobin-evolution
//   Solo Nurse  : el resto
// ==========================================
[Collection(ApiCollection.Name)]
public class PatientManagementSecurityTests
{
    private readonly HttpClient _client;
 
    public PatientManagementSecurityTests(MongoWebApplicationFactory factory)
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
 
    // ---------- 1. Sin token: todos los endpoints responden 401 ----------
 
    [Theory]
    [InlineData("POST", PatientManagementApi.RegisterUrl)]
    [InlineData("POST", PatientManagementApi.AssignNurseUrl)]
    [InlineData("POST", PatientManagementApi.MedicalRecordUrl)]
    [InlineData("POST", PatientManagementApi.HemoglobinControlUrl)]
    [InlineData("PUT", PatientManagementApi.DischargeUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/mother/some-id")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id/medical-record")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/medical-record/some-id/controls")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/mother/search/12345678")]
    [InlineData("GET", PatientManagementApi.NurseUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id/hemoglobin-evolution")]
    [InlineData("GET", PatientManagementApi.MyPatientsUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id")]
    public async Task Endpoints_WithoutToken_Returns401(string method, string url)
    {
        var response = await SendAsync(method, url);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }
 
    // ---------- 2. Endpoints de enfermera con token de madre: 403 ----------
    // ADVERTENCIA: asume que [RequireRole] responde 403 cuando el rol no coincide.
 
    [Theory]
    [InlineData("POST", PatientManagementApi.AssignNurseUrl)]
    [InlineData("POST", PatientManagementApi.MedicalRecordUrl)]
    [InlineData("POST", PatientManagementApi.HemoglobinControlUrl)]
    [InlineData("PUT", PatientManagementApi.DischargeUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/mother/some-id")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id/medical-record")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/medical-record/some-id/controls")]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/mother/search/12345678")]
    [InlineData("GET", PatientManagementApi.NurseUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id")]
    public async Task NurseEndpoints_WithMotherToken_Returns403(string method, string url)
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await SendAsync(method, url);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Forbidden, response);
    }
 
    // ---------- 3. Endpoints de madre con token de enfermera: 403 ----------
 
    [Theory]
    [InlineData("POST", PatientManagementApi.RegisterUrl)]
    [InlineData("GET", PatientManagementApi.MyPatientsUrl)]
    [InlineData("GET", PatientManagementApi.BaseUrl + "/some-id/hemoglobin-evolution")]
    public async Task MotherEndpoints_WithNurseToken_Returns403(string method, string url)
    {
        _client.UseToken(await IamAuth.GetStaffTokenAsync(_client, "Nurse"));
 
        var response = await SendAsync(method, url);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Forbidden, response);
    }
 
    // ---------- 4. Los tokens traen los claims que el controller necesita ----------
 
    [Fact]
    public async Task MotherToken_IncludesMotherIdClaim()
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await _client.GetAsync(PatientManagementApi.MyPatientsUrl);
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.DoesNotContain("Mother ID no encontrado", body);
    }
 
    [Fact]
    public async Task NurseToken_IncludesNurseIdClaim()
    {
        _client.UseToken(await IamAuth.GetStaffTokenAsync(_client, "Nurse"));
 
        var response = await _client.GetAsync(PatientManagementApi.NurseUrl);
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.DoesNotContain("Nurse ID no encontrado", body);
    }
}