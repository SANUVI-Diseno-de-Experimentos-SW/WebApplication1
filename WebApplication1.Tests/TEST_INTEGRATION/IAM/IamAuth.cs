using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;

namespace WebApplication1.Tests.TEST_INTEGRATION.IAM;

/// <summary>DNI del usuario creado y su token JWT.</summary>
public sealed record AuthSession(string Dni, string Token);


/// <summary>
/// Obtiene tokens JWT reales (registrado un usuario y haciendo login) para probar
/// endpoints protegidos de cualquier contexto
/// </summary>
public static class IamAuth
{
    public static async Task<AuthSession> CreateMotherSessionAsync(HttpClient client)
    {
        var dni = IamApi.UniqueDni();
        var created = await TestApi.PostAsync(client, IamApi.RegisterMotherUrl, IamApi.NewMother(dni));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);
 
        return new AuthSession(dni, await LoginAsync(client, dni));
    }
    
    public static async Task<AuthSession> CreateStaffSessionAsync(HttpClient client, string role = "Nurse")
    {
        var dni = IamApi.UniqueDni();
        var created = await TestApi.PostAsync(client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, role));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);
 
        return new AuthSession(dni, await LoginAsync(client, dni));
    }
    public static async Task<string> GetMotherTokenAsync(HttpClient client)
    {
        var dni = IamApi.UniqueDni();
        var created = await TestApi.PostAsync(client, IamApi.RegisterMotherUrl, IamApi.NewMother((dni)));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);

        return await LoginAsync(client, dni);
    }
    
    public static async Task<string> GetStaffTokenAsync(HttpClient client, string role = "Nurse")
    {
        var dni = IamApi.UniqueDni();
        var created = await TestApi.PostAsync(client, IamApi.RegisterStaffUrl, IamApi.NewStaff(dni, role));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);
 
        return await LoginAsync(client, dni);
    }
    
    /// <summary>Agrega el header "Authorization: Bearer ..." a todas las peticiones de este cliente.</summary>
    public static void UseToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
 
    private static async Task<string> LoginAsync(HttpClient client, string dni)
    {
        var response = await TestApi.PostAsync(client, IamApi.LoginUrl,
            new { Dni = dni, Password = IamApi.ValidPassword });
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }
}