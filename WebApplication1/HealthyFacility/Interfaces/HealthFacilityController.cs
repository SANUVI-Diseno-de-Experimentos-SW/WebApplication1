
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.HealthyFacility.Domain.Models.Commands;
using WebApplication1.HealthyFacility.Domain.Models.Queries;
using WebApplication1.HealthyFacility.Interfaces.Assemblers;
using WebApplication1.HealthyFacility.Interfaces.Facades;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.shared.Attributes;
using WebApplication1.shared.catalogs.Data;

namespace WebApplication1.HealthyFacility.Interfaces;

[ApiController]
[Route("api/health-facilities")]
[Authorize]
public class HealthFacilityController : ControllerBase
{
    private readonly HealthFacilityFacade _facade;
    private readonly DistrictRepository _districtRepository;
    private readonly IPatientRepository _patientRepository;

    public HealthFacilityController(
        HealthFacilityFacade facade,
        DistrictRepository districtRepository,
        IPatientRepository patientRepository)
    {
        _facade = facade;
        _districtRepository = districtRepository;
        _patientRepository = patientRepository;
    }

    // ==========================================
    // 1. REGISTRAR POSTA - SOLO ADMIN
    // ==========================================
    [HttpPost]
    [RequireRole("Admin")]
    public async Task<IActionResult> RegisterHealthFacility([FromBody] RegisterHealthFacilityRequest request)
    {
        try
        {
            // ✅ Log de los datos recibidos
            Console.WriteLine($"📥 Registrando posta:");
            Console.WriteLine($"  Name: {request.Name}");
            Console.WriteLine($"  Address: {request.Address}");
            Console.WriteLine($"  DistrictId: {request.DistrictId}");
            Console.WriteLine($"  Latitude: {request.Latitude}");
            Console.WriteLine($"  Longitude: {request.Longitude}");
            Console.WriteLine($"  PhoneNumber: {request.PhoneNumber}");
            Console.WriteLine($"  Services: {request.Services?.Count ?? 0}");
            Console.WriteLine($"  AvailableDays: {request.AvailableDays?.Count ?? 0}");
            Console.WriteLine($"  AvailableSlots: {request.AvailableSlots?.Count ?? 0}");

            // Validar que los datos no sean null
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest(new { error = "Name is required" });

            if (string.IsNullOrEmpty(request.Address))
                return BadRequest(new { error = "Address is required" });

            if (string.IsNullOrEmpty(request.DistrictId))
                return BadRequest(new { error = "DistrictId is required" });

            if (request.Services == null || request.Services.Count == 0)
                return BadRequest(new { error = "At least one service is required" });

            if (request.AvailableDays == null || request.AvailableDays.Count == 0)
                return BadRequest(new { error = "At least one available day is required" });

            if (request.AvailableSlots == null || request.AvailableSlots.Count == 0)
                return BadRequest(new { error = "At least one available slot is required" });

            var command = new RegisterHealthFacilityCommand(
                request.Name,
                request.Address,
                request.DistrictId,
                request.Latitude,
                request.Longitude,
                request.PhoneNumber,
                request.Services ?? new List<string>(),
                request.AvailableDays ?? new List<string>(),
                request.AvailableSlots ?? new List<string>()
            );

            await _facade.RegisterHealthFacilityAsync(command);

            return StatusCode(201, new { message = "Health facility registered successfully" }); 
        } catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // ==========================================
    // 2. ASIGNAR ENFERMERO A POSTA - SOLO ADMIN
    // ==========================================

    [HttpPost("assign-nurse")]
    [RequireRole("Admin")]
    public async Task<IActionResult> AssignNurseToFacility([FromBody] AssignNurseRequest request)
    {
        try
        {
            var command = new AssignNurseToFacilityCommand(request.FacilityId, request.NurseId);
            await _facade.AssignNurseToFacilityAsync(command);

            return Ok(new { message = "Nurse assigned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 3. RESERVAR CITA - SOLO MADRE
    // ==========================================

    [HttpPost("appointments")]
    [RequireRole("Mother")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            await _facade.ValidatePatientBelongsToMotherAsync(request.PatientId, motherId);

            var command = new BookAppointmentCommand(
                request.FacilityId,
                request.PatientId,
                motherId,
                request.AppointmentDate,
                request.AppointmentTime
            );

            await _facade.BookAppointmentAsync(command);

            return StatusCode(201, new { message = "Appointment booked successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 4. CANCELAR CITA - SOLO MADRE
    // ==========================================

    [HttpPut("appointments/cancel")]
    [RequireRole("Mother")]
    public async Task<IActionResult> CancelAppointment([FromBody] CancelAppointmentRequest request)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            await _facade.ValidateAppointmentBelongsToMotherAsync(request.AppointmentId, motherId);

            var command = new CancelAppointmentCommand(request.AppointmentId);
            await _facade.CancelAppointmentAsync(command);

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 5. LISTAR POSTAS CERCANAS - SOLO MADRE
    // ==========================================

    // En HealthFacilityController.cs - ListHealthFacilities
    [HttpGet("nearby")]
    [RequireRole("Mother")]
    public async Task<IActionResult> ListHealthFacilities([FromQuery] double lat, [FromQuery] double lng)
    {
        try
        {
            Console.WriteLine($"📍 /nearby llamado: lat={lat}, lng={lng}");
        
            var motherId = User.FindFirst("motherId")?.Value;
            Console.WriteLine($"👤 MotherId: {motherId}");

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            var query = new ListHealthFacilitiesQuery(lat, lng, motherId);
            var result = await _facade.ListHealthFacilitiesAsync(query);
        
            Console.WriteLine($"📊 Postas encontradas: {(result as List<object>)?.Count ?? 0}");
        
            // ✅ Log de la primera posta para ver estructura
            if (result is List<object> list && list.Count > 0)
            {
                var first = list[0];
                Console.WriteLine($"📋 Primera posta: {System.Text.Json.JsonSerializer.Serialize(first)}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }
    // ==========================================
    // 6. OBTENER DETALLE DE POSTA
    // ==========================================

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHealthFacilityDetail(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { error = "Facility ID is required" });
            }

            var query = new GetHealthFacilityDetailQuery(id);
            var facility = await _facade.GetHealthFacilityDetailAsync(query);

            if (facility == null)
            {
                return NotFound(new { error = "Health facility not found" });
            }

            var response = HealthFacilityDetailResourceAssembler.ToResource(facility);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 7. HISTORIAL DE CITAS - SOLO MADRE (CON PATIENTNAME)
    // ==========================================

    [HttpGet("patient/{patientId}/appointments")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetPatientAppointmentHistory(string patientId)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            await _facade.ValidatePatientBelongsToMotherAsync(patientId, motherId);

            var query = new GetPatientAppointmentHistoryQuery(patientId);
            var appointments = await _facade.GetPatientAppointmentHistoryAsync(query);

            // ✅ Enriquecer con PatientName
            var response = new List<object>();

            foreach (var appointment in appointments)
            {
                var appointmentData = appointment.ToPrimitives();

                // Obtener el paciente
                var patient = await _patientRepository.FindByIdAsync(appointmentData.PatientId);
                var patientName = patient != null
                    ? $"{patient.ToPrimitives().Name} {patient.ToPrimitives().LastName}"
                    : "Desconocido";

                // Obtener la posta
                var facility = await _facade.GetHealthFacilityDetailAsync(
                    new GetHealthFacilityDetailQuery(appointmentData.FacilityId)
                );
                var facilityName = facility?.ToPrimitives().Name ?? "Unknown";

                response.Add(new
                {
                    appointmentId = appointmentData.Id,
                    facilityName = facilityName,
                    patientId = appointmentData.PatientId,
                    patientName = patientName, // ✅ AHORA INCLUYE PATIENTNAME
                    appointmentDate = appointmentData.AppointmentDate,
                    appointmentTime = appointmentData.AppointmentTime,
                    status = appointmentData.Status
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 8. AGENDA DE ENFERMERO - SOLO ENFERMERA (CON PATIENTNAME)
    // ==========================================

    [HttpGet("appointments/nurse")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetNurseAppointmentSchedule()
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID no encontrado en el token" });
            }

            var query = new GetNurseAppointmentScheduleQuery(nurseId);
            var appointments = await _facade.GetNurseAppointmentScheduleAsync(query);

            // ✅ Enriquecer con PatientName
            var response = new List<object>();

            foreach (var appointment in appointments)
            {
                var data = appointment.ToPrimitives();

                // Obtener el paciente
                var patient = await _patientRepository.FindByIdAsync(data.PatientId);
                var patientName = patient != null
                    ? $"{patient.ToPrimitives().Name} {patient.ToPrimitives().LastName}"
                    : "Desconocido";

                response.Add(new
                {
                    appointmentId = data.Id,
                    patientId = data.PatientId,
                    patientName = patientName, // ✅ AHORA INCLUYE PATIENTNAME
                    appointmentDate = data.AppointmentDate,
                    appointmentTime = data.AppointmentTime,
                    status = data.Status
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 9. TOP CITAS - SOLO ENFERMERA (CON PATIENTNAME)
    // ==========================================

    [HttpGet("appointments/nurse/top")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetMyTopAppointments([FromQuery] int? limit = 4)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID no encontrado en el token" });
            }

            var query = new GetMyTopAppointmentsQuery(nurseId, limit);
            var appointments = await _facade.GetMyTopAppointmentsAsync(query);

            // ✅ Enriquecer con PatientName y FacilityName
            var enrichedAppointments = new List<object>();

            foreach (var appointment in appointments)
            {
                // appointment viene como objeto anónimo con propiedades
                var appointmentType = appointment.GetType();

                // Obtener patientId
                var patientId = appointmentType.GetProperty("patientId")?.GetValue(appointment)?.ToString() ?? "";

                // Obtener el paciente
                var patient = await _patientRepository.FindByIdAsync(patientId);
                var patientName = patient != null
                    ? $"{patient.ToPrimitives().Name} {patient.ToPrimitives().LastName}"
                    : "Desconocido";

                // Obtener facilityId
                var facilityId = appointmentType.GetProperty("facilityId")?.GetValue(appointment)?.ToString() ?? "";

                // Obtener la posta
                var facility = await _facade.GetHealthFacilityDetailAsync(
                    new GetHealthFacilityDetailQuery(facilityId)
                );
                var facilityName = facility?.ToPrimitives().Name ?? "Unknown";

                enrichedAppointments.Add(new
                {
                    appointmentId = appointmentType.GetProperty("appointmentId")?.GetValue(appointment)?.ToString(),
                    patientId = patientId,
                    patientName = patientName, // ✅ AHORA INCLUYE PATIENTNAME
                    facilityId = facilityId,
                    facilityName = facilityName,
                    appointmentDate = appointmentType.GetProperty("appointmentDate")?.GetValue(appointment)?.ToString(),
                    appointmentTime = appointmentType.GetProperty("appointmentTime")?.GetValue(appointment)?.ToString(),
                    status = appointmentType.GetProperty("status")?.GetValue(appointment)?.ToString()
                });
            }

            return Ok(new
            {
                success = true,
                data = enrichedAppointments,
                count = enrichedAppointments.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    // 10. MI POSTA ASIGNADA - SOLO ENFERMERA
    // ==========================================

    [HttpGet("nurse/my-facility")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetMyAssignedFacility()
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID no encontrado en el token" });
            }

            var query = new GetMyAssignedFacilityQuery(nurseId);
            var result = await _facade.GetMyAssignedFacilityAsync(query);

            if (result == null)
            {
                return NotFound(new { success = false, message = "No tienes una posta asignada actualmente" });
            }

            var facilityData = result.HealthFacility.ToPrimitives();

            return Ok(new
            {
                success = true,
                data = new
                {
                    facilityId = facilityData.Id,
                    facilityName = facilityData.Name,
                    address = facilityData.Address,
                    districtName = facilityData.DistrictName,
                    phoneNumber = facilityData.PhoneNumber,
                    status = facilityData.Status,
                    services = facilityData.Services,
                    scheduleOfOperation = facilityData.ScheduleOfOperation
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    // 11. SLOTS DISPONIBLES
    // ==========================================

    [HttpGet("{facilityId}/available-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFacilityAvailableSlots(string facilityId, [FromQuery] string date)
    {
        try
        {
            if (string.IsNullOrEmpty(facilityId))
            {
                return BadRequest(new { error = "Facility ID is required" });
            }

            if (string.IsNullOrEmpty(date))
            {
                return BadRequest(new { error = "Appointment date is required" });
            }

            var query = new GetFacilityAvailableSlotsQuery(facilityId, date);
            var slots = await _facade.GetFacilityAvailableSlotsAsync(query);

            return Ok(slots);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 12. PRÓXIMA CITA DE MADRE - SOLO MADRE
    // ==========================================

    [HttpGet("appointments/mother/next")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetMotherNextAppointment()
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID no encontrado en el token" });
            }

            var query = new GetMotherNextAppointmentQuery(motherId);
            var appointment = await _facade.GetMotherNextAppointmentAsync(query);

            if (appointment == null)
            {
                return NotFound(new { message = "No upcoming appointments found" });
            }

            var response = MotherNextAppointmentResourceAssembler.ToResource(appointment);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 13. ENFERMEROS NO ASIGNADOS - SOLO ADMIN
    // ==========================================

    [HttpGet("nurses/unassigned")]
    [RequireRole("Admin")]
    public async Task<IActionResult> ListUnassignedNurses()
    {
        try
        {
            var nurses = await _facade.ListUnassignedNursesAsync(new ListUnassignedNursesQuery());

            return Ok(new { success = true, data = nurses });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    // 14. VERIFICAR DISPONIBILIDAD - SOLO ADMIN
    // ==========================================

    [HttpGet("can-register")]
    [RequireRole("Admin")]
    public async Task<IActionResult> CanRegisterFacility()
    {
        try
        {
            var result = await _facade.CanRegisterFacilityAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 15. LISTAR TODAS POSTAS - SOLO ADMIN
    // ==========================================

    [HttpGet]
    [RequireRole("Admin")]
    public async Task<IActionResult> ListAllHealthFacilities()
    {
        try
        {
            var result = await _facade.ListAllHealthFacilitiesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 16. LISTAR DISTRITOS - SOLO ADMIN
    // ==========================================

    [HttpGet("districts")]
    [RequireRole("Admin")]
    public async Task<IActionResult> ListDistricts()
    {
        try
        {
            var districts = _districtRepository.FindAll();

            var response = districts.Select(d => new
            {
                id = d.Id,
                name = d.Name
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// ==========================================
// REQUEST DTOs
// ==========================================

public class RegisterHealthFacilityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string>? Services { get; set; }
    public List<string>? AvailableDays { get; set; }
    public List<string>? AvailableSlots { get; set; }
}

public class AssignNurseRequest
{
    public string FacilityId { get; set; } = string.Empty;
    public string NurseId { get; set; } = string.Empty;
}

public class BookAppointmentRequest
{
    public string FacilityId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string AppointmentDate { get; set; } = string.Empty;
    public string AppointmentTime { get; set; } = string.Empty;
}

public class CancelAppointmentRequest
{
    public string AppointmentId { get; set; } = string.Empty;
}