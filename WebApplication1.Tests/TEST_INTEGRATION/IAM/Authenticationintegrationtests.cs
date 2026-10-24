using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;

namespace WebApplication1.Tests.TEST_INTEGRATION.IAM;

// ==========================================
// POST /api/users/register/mother
// POST /api/users/login
// ==========================================
[Collection(ApiCollection.Name)]
public class AuthenticationIntegrationTests
{
    private readonly HttpClient _client;
 
    public AuthenticationIntegrationTests(MongoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
 
    // ---------- register/mother ----------
 
    [Fact]
    public async Task RegisterMother_ValidData_Returns201Created()
    {
        var response = await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl,
            IamApi.NewMother(IamApi.UniqueDni()));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Mother registered successfully", json.GetProperty("message").GetString());
    }
 
    [Fact]
    public async Task RegisterMother_DuplicateDni_Returns400BadRequest()
    {
        var dni = IamApi.UniqueDni();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni));
 
        var response = await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task RegisterMother_DuplicateEmail_Returns400BadRequest()
    {
        var email = IamApi.UniqueEmail();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(IamApi.UniqueDni(), email));
 
        var response = await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl,
            IamApi.NewMother(IamApi.UniqueDni(), email));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- login ----------
 
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithJwtToken()
    {
        var dni = IamApi.UniqueDni();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni));
 
        var response = await TestApi.PostAsync(_client, IamApi.LoginUrl,
            new { Dni = dni, Password = IamApi.ValidPassword });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(3, token!.Split('.').Length); // un JWT tiene 3 partes separadas por punto
    }
 
    [Fact]
    public async Task Login_WrongPassword_Returns401Unauthorized()
    {
        var dni = IamApi.UniqueDni();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni));
 
        var response = await TestApi.PostAsync(_client, IamApi.LoginUrl,
            new { Dni = dni, Password = "wrong-Pass123$" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }
 
    [Fact]
    public async Task Login_NonExistentUser_Returns401Unauthorized()
    {
        var response = await TestApi.PostAsync(_client, IamApi.LoginUrl,
            new { Dni = "00000000", Password = "anyPassword123$" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }
}