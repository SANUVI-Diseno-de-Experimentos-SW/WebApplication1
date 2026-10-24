using System.Net;
using WebApplication1.Tests.shared.Mongo;

namespace WebApplication1.Tests.TEST_INTEGRATION.IAM;

// ==========================================
// POST /api/users/password/request-code
// POST /api/users/password/verify-code
// POST /api/users/password/reset
//
// Aqui estan los casos de ERROR, que no dependen del envio de email.
// El camino feliz (pedir codigo -> verificarlo -> resetear) necesita leer el codigo que
// se "envia" por correo; para eso hay que reemplazar IEmailService por un falso en la factory.
// ==========================================
[Collection(ApiCollection.Name)]
public class PasswordResetIntegrationTests
{
    private readonly HttpClient _client;
 
    public PasswordResetIntegrationTests(MongoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
 
    // ---------- request-code ----------
 
    [Fact]
    public async Task RequestCode_NonExistentEmail_Returns400BadRequest()
    {
        var response = await TestApi.PostAsync(_client, IamApi.RequestCodeUrl,
            new { Email = IamApi.UniqueEmail() });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- verify-code ----------
 
    [Fact]
    public async Task VerifyCode_NonExistentEmail_Returns400BadRequest()
    {
        var response = await TestApi.PostAsync(_client, IamApi.VerifyCodeUrl,
            new { Email = IamApi.UniqueEmail(), Code = "123456" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task VerifyCode_WithoutRequestingCode_Returns400BadRequest()
    {
        var email = IamApi.UniqueEmail();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(IamApi.UniqueDni(), email));
 
        var response = await TestApi.PostAsync(_client, IamApi.VerifyCodeUrl,
            new { Email = email, Code = "000000" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    // ---------- reset ----------
 
    [Fact]
    public async Task ResetPassword_NonExistentEmail_Returns400BadRequest()
    {
        var response = await TestApi.PostAsync(_client, IamApi.ResetPasswordUrl,
            new { Email = IamApi.UniqueEmail(), Code = "123456", NewPassword = "newPass-123$" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task ResetPassword_InvalidCode_Returns400BadRequest()
    {
        var dni = IamApi.UniqueDni();
        var email = IamApi.UniqueEmail();
        await TestApi.PostAsync(_client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni, email));
 
        var response = await TestApi.PostAsync(_client, IamApi.ResetPasswordUrl,
            new { Email = email, Code = "000000", NewPassword = "newPass-123$" });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
 
        // la password original debe seguir funcionando
        var login = await TestApi.PostAsync(_client, IamApi.LoginUrl,
            new { Dni = dni, Password = IamApi.ValidPassword });
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, login);
    }
}