using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

/// <summary>Id y nombre de una posta creada por los tests.</summary>
public sealed record FacilityInfo(string Id, string Name);

/// <summary>
/// Reglas de la API: una enfermera debe estar asignada a una posta para recibir pacientes,
/// y cada posta admite UNA sola enfermera ("Facility already has an assigned nurse").
/// Por eso cada enfermera de los tests recibe su propia posta nueva, creada por un administrador
/// (un solo admin compartido por factory).
/// </summary>
public static class HealthFacilitySetup
{
    private sealed record AdminContext(HttpClient Client, IReadOnlyList<string> DistrictIds);

    private static readonly ConcurrentDictionary<MongoWebApplicationFactory, Lazy<Task<AdminContext>>> Admins = new();
    private static int _districtCounter = -1;

    private static Task<AdminContext> AdminAsync(MongoWebApplicationFactory factory) =>
        Admins.GetOrAdd(factory, f => new Lazy<Task<AdminContext>>(() => CreateAdminAsync(f))).Value;

    /// <summary>Cliente autenticado como administrador.</summary>
    public static async Task<HttpClient> GetAdminClientAsync(MongoWebApplicationFactory factory) =>
        (await AdminAsync(factory)).Client;

    /// <summary>Un distrito existente. Se rotan por si el dominio permite una sola posta por distrito.</summary>
    public static async Task<string> NextDistrictIdAsync(MongoWebApplicationFactory factory)
    {
        var admin = await AdminAsync(factory);
        return admin.DistrictIds[Interlocked.Increment(ref _districtCounter) % admin.DistrictIds.Count];
    }

    /// <summary>Registra una posta nueva (como admin) y devuelve su id y nombre.</summary>
    public static async Task<FacilityInfo> CreateFacilityAsync(MongoWebApplicationFactory factory,
        double latitude = -12.0464, double longitude = -77.0428)
    {
        var admin = await GetAdminClientAsync(factory);
        var districtId = await NextDistrictIdAsync(factory);

        // 1. Registrar la posta (la respuesta solo trae un mensaje, sin id)
        var name = $"Posta Test {Guid.NewGuid():N}";
        var created = await TestApi.PostAsync(admin, HealthFacilityApi.BaseUrl,
            HealthFacilityApi.NewFacility(name, districtId, latitude, longitude));
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
                    if (!string.IsNullOrEmpty(id)) return new FacilityInfo(id, name);
                }
            }
        }

        Assert.Fail($"No encontre la posta '{name}' en GET health-facilities. Respuesta: {list}");
        return new FacilityInfo(string.Empty, name);
    }

    /// <summary>Intenta asignar la enfermera a la posta y devuelve la respuesta sin verificarla.</summary>
    public static async Task<HttpResponseMessage> TryAssignNurseAsync(MongoWebApplicationFactory factory,
        string nurseToken, string facilityId)
    {
        var nurseId = JwtClaims.Get(nurseToken, "nurseId");
        if (string.IsNullOrEmpty(nurseId))
            Assert.Fail("El token de la enfermera no trae el claim 'nurseId'.");

        var admin = await GetAdminClientAsync(factory);
        return await TestApi.PostAsync(admin, HealthFacilityApi.AssignNurseUrl,
            new { FacilityId = facilityId, NurseId = nurseId });
    }

    /// <summary>Asigna la enfermera a la posta y verifica que la API responda 200.</summary>
    public static async Task AssignNurseToFacilityAsync(MongoWebApplicationFactory factory,
        string nurseToken, string facilityId)
    {
        var response = await TryAssignNurseAsync(factory, nurseToken, facilityId);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
    }

    /// <summary>Crea una posta nueva, le asigna la enfermera y devuelve el id de la posta.</summary>
    public static async Task<string> AssignNurseToNewFacilityAsync(MongoWebApplicationFactory factory, string nurseToken)
    {
        var facility = await CreateFacilityAsync(factory);
        await AssignNurseToFacilityAsync(factory, nurseToken, facility.Id);
        return facility.Id;
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
}