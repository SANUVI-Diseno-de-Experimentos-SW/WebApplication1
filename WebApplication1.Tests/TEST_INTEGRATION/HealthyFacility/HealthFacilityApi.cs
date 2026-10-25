namespace WebApplication1.Tests.TEST_INTEGRATION.HealthyFacility;

public class HealthFacilityApi
{
    public const string BaseUrl = "/api/health-facilities";
 
    public const string AssignNurseUrl = BaseUrl + "/assign-nurse";
    public const string DistrictsUrl = BaseUrl + "/districts";
 
    // ADVERTENCIA: si tu dominio valida estos textos (servicios, dias, horarios), ajustalos aqui.
    // Si el registro de la posta responde 400, el mensaje del error indica que valor rechazo.
    public static readonly string[] ValidServices = { "Control de hemoglobina" };
    public static readonly string[] ValidDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    public static readonly string[] ValidSlots = { "08:00", "09:00", "10:00" };
 
    public static object NewFacility(string name, string districtId) => new
    {
        Name = name,
        Address = "Av. Test 123",
        DistrictId = districtId,
        Latitude = -12.0464,
        Longitude = -77.0428,
        PhoneNumber = "987654321",
        Services = ValidServices,
        AvailableDays = ValidDays,
        AvailableSlots = ValidSlots
    };
}