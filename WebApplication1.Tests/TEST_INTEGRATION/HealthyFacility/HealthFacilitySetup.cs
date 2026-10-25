using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

/// <summary>
/// Regla de la API: una enfermera debe estar asignada a una posta para recibir pacientes,
/// y cada posta admite UNA sola enfermera ("Facility already has an assigned nurse").
/// Por eso cada enfermera de los tests recibe su propia posta nueva, creada por un administrador
/// (un solo admin compartido por factory).
/// </summary>
public static class HealthFacilitySetup
{
    private sealed record AdminContext(HttpClient Client, IReadOnlyList<string> DistrictIds);
 
    private static readonly ConcurrentDictionary<MongoWebApplicationFactory, Lazy<Task<AdminContext>>> Admins = new();
    private static int _districtCounter = -1;
 
    /// <summary>Crea una posta nueva y le asigna la enfermera (identificada por su token).</summary>
    public static async Task AssignNurseToNewFacilityAsync(MongoWebApplicationFactory factory, string nurseToken)
    {
        var nurseId = JwtClaims.Get(nurseToken, "nurseId");
        if (string.IsNullOrEmpty(nurseId))
            Assert.Fail("El token de la enfermera no trae el claim 'nurseId'.");
 
        var admin = await Admins.GetOrAdd(factory,
            f => new Lazy<Task<AdminContext>>(() => CreateAdminAsync(f))).Value;
 
        // Se rotan los distritos por si el dominio permite una sola posta por distrito.
        var districtId = admin.DistrictIds[Interlocked.Increment(ref _districtCounter) % admin.DistrictIds.Count];
        var facilityId = await CreateFacilityAsync(admin.Client, districtId);
 
        var response = await TestApi.PostAsync(admin.Client, HealthFacilityApi.AssignNurseUrl,
            new { FacilityId = facilityId, NurseId = nurseId });
 
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }
 
    private static async Task<AdminContext> CreateAdminAsync(MongoWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        client.UseToken(await IamAuth.GetStaffTokenAsync(client, "Admin"));
 
        var response = await client.GetAsync(HealthFacilityApi.DistrictsUrl);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
 
        var districts = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = new List<string>();
        if (districts.ValueKind == JsonValueKind.Array)
        {
            foreach (var district in districts.EnumerateArray())
            {
                var id = district.FindString("id");
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
            }
        }
 
        if (ids.Count == 0)
            Assert.Fail($"No pude leer distritos de GET districts. Respuesta: {districts}");
 
        return new AdminContext(client, ids);
    }
 
    private static async Task<string> CreateFacilityAsync(HttpClient admin, string districtId)
    {
        // 1. Registrar la posta (la respuesta solo trae un mensaje, sin id)
        var name = $"Posta Test {Guid.NewGuid():N}";
        var created = await TestApi.PostAsync(admin, HealthFacilityApi.BaseUrl,
            HealthFacilityApi.NewFacility(name, districtId));
        await TestApi.AssertStatusAsync(HttpStatusCode.Created, created);
 
        // 2. Buscar su id en la lista de postas, por el nombre unico
        var listResponse = await admin.GetAsync(HealthFacilityApi.BaseUrl);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, listResponse);
 
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = list.ValueKind == JsonValueKind.Array
            ? list
            : list.Find("data", "facilities", "healthFacilities");
 
        if (items is { ValueKind: JsonValueKind.Array } array)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (item.FindString("name", "facilityName") == name)
                {
                    var id = item.FindString("id", "facilityId");
                    if (!string.IsNullOrEmpty(id)) return id;
                }
            }
        }
 
        Assert.Fail($"No encontre la posta '{name}' en GET health-facilities. Respuesta: {list}");
        return string.Empty;
    }
}