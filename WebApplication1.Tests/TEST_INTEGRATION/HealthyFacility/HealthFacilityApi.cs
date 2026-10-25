namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

/// <summary>URLs y datos de prueba del contexto HealthyFacility (api/health-facilities).</summary>
public static class HealthFacilityApi
{
    public const string BaseUrl = "/api/health-facilities";
 
    public const string AssignNurseUrl = BaseUrl + "/assign-nurse";
    public const string DistrictsUrl = BaseUrl + "/districts";
    public const string AppointmentsUrl = BaseUrl + "/appointments";
    public const string CancelAppointmentUrl = BaseUrl + "/appointments/cancel";
    public const string NearbyUrl = BaseUrl + "/nearby";
    public const string NurseAppointmentsUrl = BaseUrl + "/appointments/nurse";
    public const string MotherNextAppointmentUrl = BaseUrl + "/appointments/mother/next";

    public static string FacilityUrl(string id) => $"{BaseUrl}/{id}";
    public static string PatientAppointmentsUrl(string patientId) => $"{BaseUrl}/patient/{patientId}/appointments";
    public static string NearbyQuery(double lat, double lng) =>
        FormattableString.Invariant($"{NearbyUrl}?lat={lat}&lng={lng}");

    // ADVERTENCIA: si tu dominio valida estos textos (servicios, dias, horarios), ajustalos aqui.
    // Si el registro de la posta responde 400, el mensaje del error indica que valor rechazo.
    public static readonly string[] ValidServices = { "Control de hemoglobina" };
    public static readonly string[] ValidDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    public static readonly string[] ValidSlots = { "08:00", "09:00", "10:00" };

    public const string ValidTime = "08:00";
    public const string FacilityAddress = "Av. Test 123";

    public static object NewFacility(string name, string districtId,
        double latitude = -12.0464, double longitude = -77.0428) => new
    {
        Name = name,
        Address = FacilityAddress,
        DistrictId = districtId,
        Latitude = latitude,
        Longitude = longitude,
        PhoneNumber = "987654321",
        Services = ValidServices,
        AvailableDays = ValidDays,
        AvailableSlots = ValidSlots
    };

    /// <summary>Proxima fecha entre lunes y viernes (dentro de los dias de atencion), con margen de 2 dias.</summary>
    public static string NextWeekdayDate() => WeekdayFrom(DateTime.UtcNow.Date.AddDays(2));

    /// <summary>Una fecha pasada que cae entre lunes y viernes.</summary>
    public static string PastWeekdayDate() => WeekdayFrom(DateTime.UtcNow.Date.AddDays(-14));

    private static string WeekdayFrom(DateTime date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(1);

        return date.ToString("yyyy-MM-dd");
    }

    public static object NewAppointment(string facilityId, string patientId, string? date = null, string? time = null) => new
    {
        FacilityId = facilityId,
        PatientId = patientId,
        AppointmentDate = date ?? NextWeekdayDate(),
        AppointmentTime = time ?? ValidTime
    };
}