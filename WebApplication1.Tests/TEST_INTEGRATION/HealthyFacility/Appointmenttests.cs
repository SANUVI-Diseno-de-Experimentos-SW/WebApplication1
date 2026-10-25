using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Tests.shared.Mongo;
using WebApplication1.Tests.TEST_INTEGRATION.IAM;
using WebApplication1.Tests.TEST_INTEGRATION.PatientManagement;

namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

// ==========================================
// POST appointments               (Mother)  reservar cita
// PUT  appointments/cancel        (Mother)  cancelar cita
// GET  appointments/nurse         (Nurse)   agenda de la enfermera
// GET  appointments/mother/next   (Mother)  proxima cita
// (para leer el id de la cita se usa GET patient/{id}/appointments)
// ==========================================
[Collection(ApiCollection.Name)]
public class AppointmentTests
{
    private readonly MongoWebApplicationFactory _factory;

    public AppointmentTests(MongoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ---------- POST appointments ----------

    [Fact]
    public async Task BookAppointment_ValidData_Returns201()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Appointment booked successfully", json.FindString("message"));
    }

    // ADVERTENCIA: asume formato de fecha "yyyy-MM-dd" y hora "HH:mm". Si la reserva da 400,
    // el mensaje del error indica que formato espera tu servicio.
    [Fact]
    public async Task BookAppointment_AppearsInPatientHistory()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();

        var history = await scenario.GetHistoryAsync();

        Assert.Equal(1, history.GetArrayLength());
        var item = history[0];
        Assert.Equal(scenario.Patient.PatientId, item.FindString("patientId"));
        Assert.Equal("Baby Doe", item.FindString("patientName"));
        Assert.Contains(scenario.Time, item.FindString("appointmentTime") ?? "");
    }

    [Fact]
    public async Task BookAppointment_PatientOfAnotherMother_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        var otherMother = await PatientScenario.CreateAsync(_factory);

        // La madre del escenario intenta reservar para el paciente de otra madre
        var response = await scenario.BookRawAsync(patientId: otherMother.PatientId);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("no pertenece", body);
    }

    [Fact]
    public async Task BookAppointment_NonExistentFacility_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(facilityId: "000000000000000000000000");
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Facility not found", body);
    }

    [Theory]
    [InlineData("fecha-invalida")]
    [InlineData("25/12/2030")]
    [InlineData("")]
    public async Task BookAppointment_InvalidDate_Returns400(string date)
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(date: date);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Invalid appointment date", body);
    }

    [Theory]
    [InlineData("hora-invalida")]
    [InlineData("25:99")]
    [InlineData("")]
    public async Task BookAppointment_InvalidTime_Returns400(string time)
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(time: time);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("Invalid appointment time", body);
    }

    [Fact]
    public async Task BookAppointment_PastDate_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(date: HealthFacilityApi.PastWeekdayDate());
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("must be in the future", body);
    }

    // Una posta sin enfermera asignada no puede recibir citas
    [Fact]
    public async Task BookAppointment_FacilityWithoutNurse_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        var emptyFacility = await HealthFacilitySetup.CreateFacilityAsync(_factory);

        var response = await scenario.BookRawAsync(facilityId: emptyFacility.Id);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("no assigned nurse", body);
    }

    // Un horario ya reservado no se puede reservar dos veces.
    [Fact]
    public async Task BookAppointment_SlotAlreadyTaken_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(otherMother.MotherClient, otherMother.PatientId);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
        Assert.Contains("already reserved", body);
    }

    // ADVERTENCIA: al cancelar una cita, su horario deberia quedar libre. El servicio no revisa
    // el estado de la cita existente, asi que depende de FindByFacilityAndDateTimeAsync.
    // Si recibes 400 "already reserved", las citas canceladas siguen bloqueando el horario.
    [Fact]
    public async Task BookAppointment_SlotOfCancelledAppointment_CanBeBookedAgain()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var appointmentId = await scenario.GetAppointmentIdAsync();
        await scenario.Patient.MotherClient.PutAsJsonAsync(
            HealthFacilityApi.CancelAppointmentUrl, new { AppointmentId = appointmentId });
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await scenario.BookRawAsync(otherMother.MotherClient, otherMother.PatientId);

        await TestApi.AssertStatusAsync(HttpStatusCode.Created, response);
    }

    // ---------- PUT appointments/cancel ----------

    [Fact]
    public async Task CancelAppointment_OwnAppointment_Returns200_AndStatusChanges()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var appointmentId = await scenario.GetAppointmentIdAsync();

        var response = await scenario.Patient.MotherClient.PutAsJsonAsync(
            HealthFacilityApi.CancelAppointmentUrl, new { AppointmentId = appointmentId });

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Appointment cancelled successfully", json.FindString("message"));

        // El estado de la cita cambia a cancelada
        var history = await scenario.GetHistoryAsync();
        var status = history[0].FindString("status") ?? "";
        Assert.Contains("CANCEL", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelAppointment_OfAnotherMother_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var appointmentId = await scenario.GetAppointmentIdAsync();
        var otherMother = await PatientScenario.CreateAsync(_factory);

        var response = await otherMother.MotherClient.PutAsJsonAsync(
            HealthFacilityApi.CancelAppointmentUrl, new { AppointmentId = appointmentId });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact]
    public async Task CancelAppointment_NonExistent_Returns400()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.Patient.MotherClient.PutAsJsonAsync(
            HealthFacilityApi.CancelAppointmentUrl, new { AppointmentId = "000000000000000000000000" });

        await TestApi.AssertStatusAsync(HttpStatusCode.BadRequest, response);
    }

    // ---------- GET appointments/nurse ----------

    [Fact]
    public async Task NurseSchedule_WithoutAppointments_ReturnsEmptyList()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.NurseClient.GetAsync(HealthFacilityApi.NurseAppointmentsUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, json.GetArrayLength());
    }

    [Fact]
    public async Task NurseSchedule_AfterBooking_ShowsTheAppointment()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var appointmentId = await scenario.GetAppointmentIdAsync();

        var response = await scenario.NurseClient.GetAsync(HealthFacilityApi.NurseAppointmentsUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, json.GetArrayLength());
        Assert.Equal(appointmentId, json[0].FindString("appointmentId", "id"));
        Assert.Equal("Baby Doe", json[0].FindString("patientName"));
    }

    // Cada enfermera solo ve las citas de su propia posta
    [Fact]
    public async Task NurseSchedule_OtherNurse_DoesNotSeeTheAppointment()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var (otherNurse, _) = await PatientScenario.CreateNurseAsync(_factory);

        var response = await otherNurse.GetAsync(HealthFacilityApi.NurseAppointmentsUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, json.GetArrayLength());
    }

    // ---------- GET appointments/mother/next ----------

    [Fact]
    public async Task NextAppointment_WithoutAppointments_Returns404()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);

        var response = await scenario.Patient.MotherClient.GetAsync(HealthFacilityApi.MotherNextAppointmentUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.NotFound, response);
    }

    [Fact]
    public async Task NextAppointment_AfterBooking_Returns200()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();

        var response = await scenario.Patient.MotherClient.GetAsync(HealthFacilityApi.MotherNextAppointmentUrl);
        var body = await response.Content.ReadAsStringAsync();

        await TestApi.AssertStatusAsync(HttpStatusCode.OK, response);
        Assert.Contains(scenario.Time, body);
    }

    // ADVERTENCIA: asume que "proxima cita" no incluye las canceladas.
    [Fact]
    public async Task NextAppointment_AfterCancelling_Returns404()
    {
        var scenario = await AppointmentScenario.CreateAsync(_factory);
        await scenario.BookAsync();
        var appointmentId = await scenario.GetAppointmentIdAsync();
        await scenario.Patient.MotherClient.PutAsJsonAsync(
            HealthFacilityApi.CancelAppointmentUrl, new { AppointmentId = appointmentId });

        var response = await scenario.Patient.MotherClient.GetAsync(HealthFacilityApi.MotherNextAppointmentUrl);

        await TestApi.AssertStatusAsync(HttpStatusCode.NotFound, response);
    }
}