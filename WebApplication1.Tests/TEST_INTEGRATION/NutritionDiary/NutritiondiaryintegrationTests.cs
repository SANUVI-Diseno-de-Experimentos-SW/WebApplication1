using System.Net;
using System.Net.Http.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.NutritionDiary;

[Collection(ApiCollection.Name)]
public class NutritiondiaryintegrationTests
{
    private readonly HttpClient _client;

    public NutritiondiaryintegrationTests(MongoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    // Peticion a un endpoint "solo Mother" con datos cualquiera
    private Task<HttpResponseMessage> SendAsync(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST")
            request.Content = JsonContent.Create(new { PatientId = "patient-1", FoodItemId = "food-1", Quantity = 1.0 });
 
        return _client.SendAsync(request);
    }
    
    // ---------- 1. Seguridad ----------
 
    [Theory]
    [InlineData("POST", NutritionDiaryApi.FoodEntryUrl)]
    [InlineData("GET", NutritionDiaryApi.BaseUrl + "/today/patient-1")]
    [InlineData("GET", NutritionDiaryApi.BaseUrl + "/history/patient-1")]
    public async Task MotherEndpoints_WithoutToken_Returns401(string method, string url)
    {
        var response = await SendAsync(method, url);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Unauthorized, response);
    }
    
    // ADVERTENCIA: asume que [RequireRole] responde 403 cuando el rol no coincide.
    // Si tu atributo devuelve otro codigo
    [Theory]
    [InlineData("POST", NutritionDiaryApi.FoodEntryUrl)]
    [InlineData("GET", NutritionDiaryApi.BaseUrl + "/today/patient-1")]
    [InlineData("GET", NutritionDiaryApi.BaseUrl + "/history/patient-1")]
    public async Task MotherEndpoints_WithStaffToken_Returns403(string method, string url)
    {
        _client.UseToken(await IamAuth.GetStaffTokenAsync(_client, "Nurse"));
 
        var response = await SendAsync(method, url);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.Forbidden, response);
    }
    
    // Comprueba que el token de una madre trae el claim "motherId" que el controller necesita.
    // Si falla, ningun endpoint de madre funcionara aunque el login sea correcto.
    [Fact]
    public async Task MotherToken_IncludesMotherIdClaim()
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await _client.GetAsync(NutritionDiaryApi.HistoryUrl("patient-inexistente"));
        var body = await response.Content.ReadAsStringAsync();
 
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("Mother ID no encontrado", body);
    }
    
    // ---------- 2. POST food-entry: validaciones ----------
 
    [Fact]
    public async Task RegisterFoodEntry_MissingFields_Returns400()
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await _client.PostAsJsonAsync(NutritionDiaryApi.FoodEntryUrl,
            new { PatientId = "", FoodItemId = "", Quantity = 0 });
        var body = await response.Content.ReadAsStringAsync();
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Faltan campos requeridos", body);
    }
 
    [Fact]
    public async Task RegisterFoodEntry_NegativeQuantity_Returns400()
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await _client.PostAsJsonAsync(NutritionDiaryApi.FoodEntryUrl,
            new { PatientId = "patient-1", FoodItemId = "food-1", Quantity = -5 });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
    
    // ---------- 3. GET today / history: paciente que no es de la madre ----------
 
    [Theory]
    [InlineData("today")]
    [InlineData("history")]
    public async Task MotherEndpoints_PatientNotFoundOrNotOwned_Returns400(string endpoint)
    {
        _client.UseToken(await IamAuth.GetMotherTokenAsync(_client));
 
        var response = await _client.GetAsync($"{NutritionDiaryApi.BaseUrl}/{endpoint}/000000000000000000000000");
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
    
    // ---------- 4. GET foods/search (publico) ----------
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchFoods_EmptyText_Returns400(string text)
    {
        var response = await _client.GetAsync($"{NutritionDiaryApi.SearchFoodsUrl}?text={Uri.EscapeDataString(text)}");
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task SearchFoods_WithoutTextParameter_Returns400()
    {
        var response = await _client.GetAsync(NutritionDiaryApi.SearchFoodsUrl);
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
 
    [Fact]
    public async Task SearchFoods_ValidText_Returns200WithoutToken()
    {
        var response = await _client.GetAsync($"{NutritionDiaryApi.SearchFoodsUrl}?text=arroz");
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }
    
    // ---------- 5. GET foods/{foodItemId} (publico) ----------
 
    // ADVERTENCIA: asume que un alimento inexistente lanza excepcion (-> 400).
    // Si tu servicio devuelve null, recibiras 204 y habra que ajustar este esperado.
    [Theory]
    [InlineData("000000000000000000000000")]
    [InlineData("id-que-no-existe")]
    public async Task GetFoodDetails_NonExistentId_Returns400(string foodItemId)
    {
        var response = await _client.GetAsync(NutritionDiaryApi.FoodDetailsUrl(foodItemId));
 
        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }
}