using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;

namespace WebApplication1.Tests.TEST_INTEGRATION.IAM;

// ==========================================
// POST /api/users/register/staff
// ==========================================
[Collection(ApiCollection.Name)]
public class StaffRegistrationIntegrationTests
{
    private readonly HttpClient _client;

    public StaffRegistrationIntegrationTests(MongoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ADVERTENCIA: confirma como acepta tu RoleExtensions.FromString los textos de rol
    // ("Nurse", "nurse", "NURSE"...). Si falla con 400 por rol invalido, ajusta estos valores.
    [Theory]
    [InlineData("Nurse")]
    [InlineData("Admin")]
    public async Task CreateStaff_ValidRole_Returns201Created(string role)
    {
        var response = await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl,
            IamApi.NewStaff(IamApi.UniqueDni(), role));

        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Staff user registered successfully", json.GetProperty("message").GetString());
    }

    [Fact]
    public async Task CreateStaff_InvalidRole_Returns400BadRequest()
    {
        var response = await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl,
            IamApi.NewStaff(IamApi.UniqueDni(), "Wizard"));

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // OJO: este test puede revelar una inconsistencia real. RegisterMother captura
    // InvalidOperationException (-> 400), pero CreateStaffUser solo captura ArgumentException.
    // Si el servicio lanza InvalidOperationException por DNI duplicado, aqui recibiras 500.
    [Fact]
    public async Task CreateStaff_DuplicateDni_Returns400BadRequest()
    {
        var dni = IamApi.UniqueDni();
        await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, "Nurse"));

        var response = await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, "Nurse"));

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact]
    public async Task CreateStaff_ThenLogin_Returns200WithToken()
    {
        var dni = IamApi.UniqueDni();
        await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, "Nurse"));

        var response = await TestApi.PostAsync(_client, IamApi.LoginUrl,
            new { Dni = dni, Password = IamApi.ValidPassword });

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }
    
    [Theory]
    [InlineData("Nurse")]
    [InlineData("Admin")]
    public async Task CreateStaff_ThenLogin_Returns200WithValidTokenAndRole(string role)
    {
        var dni = IamApi.UniqueDni();
    
        // 1. Registrar y hacer login
        await TestApi.PostAsync(_client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, role));
        var loginResponse = await TestApi.PostAsync(_client, IamApi.LoginUrl, new { Dni = dni, Password = IamApi.ValidPassword });
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, loginResponse);

        // 2. Extraer el token de la respuesta
        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString()!;

        // 3. Leer los claims con la librería oficial
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
    
        // 4. Validar el rol (busca en el claim estándar de .NET o en el nombre corto)
        var userRole = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;
    
        Assert.Equal(role, userRole);
    }
    
}
