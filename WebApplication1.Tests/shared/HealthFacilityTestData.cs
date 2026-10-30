using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

namespace WebApplication1.Tests.HealthyFacility;

/// <summary>
/// Fabrica compartida de objetos para las pruebas de HealthFacility.
///
/// IMPORTANTE: los constructores de Coordinates, OperatingSchedule y NurseAssignment
/// estan SUPUESTOS (no conozco esas clases). Si alguno se crea distinto, ajustalo
/// SOLO en la seccion "SUPUESTOS": las pruebas no cambian.
/// </summary>
internal static class HealthFacilityTestData
{
    // ==========================================
    // SUPUESTOS (ajustar aqui si no compilan)
    // ==========================================

    public static Coordinates NewCoordinates() => new(-12.0464, -77.0428);

    // OperatingSchedule recibe dos listas de texto (segun el error del compilador).
    public static OperatingSchedule NewOperatingSchedule() => new(
        new List<string> { "08:00-17:00" },
        new List<string> { "09:00-13:00" });

    // NurseAssignment recibe tres textos y el segundo es facilityId (segun el compilador).
    // No se el orden exacto, asi que el nurseId va en la primera y la tercera posicion:
    // sea cual sea la que guarde NurseId, las pruebas de AssignNurse siguen distinguiendo
    // a las enfermeras. Si no, pasame NurseAssignment.cs y lo dejo exacto.
    public static NurseAssignment NewNurseAssignment(string nurseId = "nurse-001") =>
        new(nurseId, "facility-001", nurseId);

    // ==========================================
    // AGREGADO
    // ==========================================

    public static List<string> NewServices() => new() { "Control CRED", "Vacunacion" };

    public static HealthFacility NewFacility(
        string id = "facility-001",
        string name = "Centro de Salud Central",
        string address = "Av. Principal 123",
        string districtId = "district-001",
        string districtName = "San Isidro",
        Coordinates? coordinates = null,
        string phoneNumber = "014567890",
        List<string>? services = null,
        OperatingSchedule? operatingSchedule = null,
        string scheduleOfOperation = "Lunes a viernes",
        FacilityStatus status = FacilityStatus.ACTIVE,
        List<NurseAssignment>? nurseAssignments = null)
    {
        return new HealthFacility(
            id,
            name,
            address,
            districtId,
            districtName,
            coordinates ?? NewCoordinates(),
            phoneNumber,
            services ?? NewServices(),
            operatingSchedule ?? NewOperatingSchedule(),
            scheduleOfOperation,
            status,
            nurseAssignments ?? new List<NurseAssignment>());
    }
}
