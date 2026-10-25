using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

// ==========================================
// POST api/health-facilities          (Admin)
// POST assign-nurse                   (Admin)
// GET  {id}                           (publico)
// GET  nearby                         (Mother)
// ==========================================
[Collection(ApiCollection.Name)]
public class HealthFacilityRegistrationTests
{
    private readonly MongoWebApplicationFactory _factory;

    public HealthFacilityRegistrationTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, string Token)> NewNurseAsync()
    {
        var client = _factory.CreateClient();
        var session = await IamAuth.CreateStaffSessionAsync(client, "Nurse");
        client.UseToken(session.Token);
        return (client, session.Token);
    }

    // ---------- POST register facility ----------

    [Fact]
    public async Task RegisterFacility_ValidData_Returns201_AndAppearsInTheList()
    {
        var admin = await HealthFacilitySetup.GetAdminClientAsync(_factory);
        var districtId = await HealthFacilitySetup.NextDistrictIdAsync(_factory);
        var name = $"Posta Registro {Guid.NewGuid():N}";

        var response = await TestApi.PostAsync(admin, HealthFacilityApi.BaseUrl,
            HealthFacilityApi.NewFacility(name, districtId));

        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);

        var list = await admin.GetAsync(HealthFacilityApi.BaseUrl);
        await TestApi.AssertStatusAsync(HttpStatusCode.OK, list);
        Assert.Contains(name, await list.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("name", "Name is required")]
    [InlineData("address", "Address is required")]
    [InlineData("districtId", "DistrictId is required")]
    [InlineData("services", "service")]
    [InlineData("availableDays", "day")]
    [InlineData("availableSlots", "slot")]
    public async Task RegisterFacility_MissingRequiredField_Returns400(string field, string expectedMessagePart)
    {
        var admin = await HealthFacilitySetup.GetAdminClientAsync(_factory);
        var districtId = await HealthFacilitySetup.NextDistrictIdAsync(_factory);

        var body = new Dictionary<string, object?>
        {
            ["name"] = $"Posta Invalida {Guid.NewGuid():N}",
            ["address"] = HealthFacilityApi.FacilityAddress,
            ["districtId"] = districtId,
            ["latitude"] = -12.0464,
            ["longitude"] = -77.0428,
            ["phoneNumber"] = "987654321",
            ["services"] = HealthFacilityApi.ValidServices,
            ["availableDays"] = HealthFacilityApi.ValidDays,
            ["availableSlots"] = HealthFacilityApi.ValidSlots
        };
        body[field] = field is "services" or "availableDays" or "availableSlots"
            ? Array.Empty<string>()
            : "";

        var response = await TestApi.PostAsync(admin, HealthFacilityApi.BaseUrl, body);
        var text = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains(expectedMessagePart, text, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- POST assign-nurse ----------

    [Fact]
    public async Task AssignNurse_FreeFacility_Returns200()
    {
        var facility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var (_, nurseToken) = await NewNurseAsync();

        var response = await HealthFacilitySetup.TryAssignNurseAsync(_factory, nurseToken, facility.Id);

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Nurse assigned successfully", json.FindString("message"));
    }

    // Regla descubierta en los tests: cada posta admite una sola enfermera.
    [Fact]
    public async Task AssignNurse_FacilityAlreadyHasNurse_Returns400()
    {
        var facility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var (_, firstNurse) = await NewNurseAsync();
        var (_, secondNurse) = await NewNurseAsync();
        await HealthFacilitySetup.AssignNurseToFacilityAsync(_factory, firstNurse, facility.Id);

        var response = await HealthFacilitySetup.TryAssignNurseAsync(_factory, secondNurse, facility.Id);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("already has an assigned nurse", body);
    }

    [Fact]
    public async Task AssignNurse_NonExistentFacility_Returns400()
    {
        var (_, nurseToken) = await NewNurseAsync();

        var response = await HealthFacilitySetup.TryAssignNurseAsync(_factory, nurseToken, "000000000000000000000000");
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Facility not found", body);
    }

    [Fact]
    public async Task AssignNurse_NonExistentNurse_Returns400()
    {
        var facility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var admin = await HealthFacilitySetup.GetAdminClientAsync(_factory);

        var response = await TestApi.PostAsync(admin, HealthFacilityApi.AssignNurseUrl,
            new { FacilityId = facility.Id, NurseId = "000000000000000000000000" });
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Nurse not found", body);
    }

    // Regla del servicio: una enfermera solo puede estar en una posta.
    [Fact]
    public async Task AssignNurse_NurseAlreadyAssignedToAnotherFacility_Returns400()
    {
        var firstFacility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var secondFacility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var (_, nurseToken) = await NewNurseAsync();
        await HealthFacilitySetup.AssignNurseToFacilityAsync(_factory, nurseToken, firstFacility.Id);

        var response = await HealthFacilitySetup.TryAssignNurseAsync(_factory, nurseToken, secondFacility.Id);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("already assigned to another facility", body);
    }

    // ---------- GET {id} (publico) ----------

    [Fact]
    public async Task GetFacility_Existing_ReturnsDetailWithoutToken()
    {
        var facility = await HealthFacilitySetup.CreateFacilityAsync(_factory);
        var anonymous = _factory.CreateClient(); // sin token

        var response = await anonymous.GetAsync(HealthFacilityApi.FacilityUrl(facility.Id));
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains(facility.Name, body);
        Assert.Contains(HealthFacilityApi.FacilityAddress, body);
    }

    // 404 si el servicio devuelve null, o 400 si lanza una excepcion.
    [Theory]
    [InlineData("000000000000000000000000")]
    [InlineData("id-que-no-existe")]
    public async Task GetFacility_NonExistent_Returns404Or400(string id)
    {
        var response = await _factory.CreateClient().GetAsync(HealthFacilityApi.FacilityUrl(id));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.BadRequest });
    }

    // ---------- GET nearby ----------

    // Regla de la API: la madre debe tener al menos un paciente registrado para usar "nearby".
    [Fact]
    public async Task Nearby_MotherWithoutPatients_Returns400()
    {
        var mother = _factory.CreateClient();
        mother.UseToken(await IamAuth.GetMotherTokenAsync(mother));

        var response = await mother.GetAsync(HealthFacilityApi.NearbyQuery(-12.0464, -77.0428));
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("al menos un paciente", body);
    }

    // Se crea la posta en coordenadas propias (lejos de las demas) para que sea la mas cercana
    // y no se pierda entre las postas de otros tests. Se le asigna una enfermera por si la API
    // solo lista postas activas.
    // ADVERTENCIA: la forma de la respuesta y el radio dependen de tu servicio; si falla,
    // el mensaje muestra la respuesta completa.
    [Fact]
    public async Task Nearby_FacilityAtTheSameLocation_IsReturned()
    {
        const double lat = -3.7491, lng = -73.2538;
        var facility = await HealthFacilitySetup.CreateFacilityAsync(_factory, lat, lng);
        var (_, nurseToken) = await NewNurseAsync();
        await HealthFacilitySetup.AssignNurseToFacilityAsync(_factory, nurseToken, facility.Id);

        var patient = await PatientScenario.CreateAsync(_factory); // madre con un paciente registrado

        var response = await patient.MotherClient.GetAsync(HealthFacilityApi.NearbyQuery(lat, lng));
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.True(body.Contains(facility.Name), $"La posta '{facility.Name}' no aparece en nearby. Respuesta: {body}");
    }
}