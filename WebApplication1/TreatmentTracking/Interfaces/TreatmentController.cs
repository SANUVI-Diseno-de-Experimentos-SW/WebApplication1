using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.shared.Attributes;
using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Domain.Model.Queries;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Interfaces.Assemblers;
using WebApplication1.TreatmentTracking.Interfaces.Facades;
using WebApplication1.TreatmentTracking.Interfaces.Resources;

namespace WebApplication1.TreatmentTracking.Interfaces;


[ApiController]
[Route("api/treatment-tracking")]
[Authorize]
public class TreatmentController : ControllerBase
{
    private readonly TreatmentFacade _facade;

    public TreatmentController(TreatmentFacade facade)
    {
        _facade = facade;
    }

    // ==========================================
    // 1. INICIAR TRATAMIENTO - SOLO NURSE
    // ==========================================

    [HttpPost("treatments")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> StartTreatment([FromBody] StartTreatmentRequest request)
    {
        try
        {
            // 🔍 LOG PARA DEPURAR
            Console.WriteLine($"Request recibido: {System.Text.Json.JsonSerializer.Serialize(request)}");
        
            if (request == null)
            {
                return BadRequest(new { error = "El cuerpo de la petición es requerido" });
            }

            if (string.IsNullOrEmpty(request.PatientId))
            {
                return BadRequest(new { error = "PatientId es requerido" });
            }

            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID no encontrado en el token" });
            }

            // Validar que la enfermera tiene este paciente
            await _facade.ValidateNurseHasPatientAsync(nurseId, request.PatientId);

            var resource = new StartTreatmentResource
            {
                PatientId = request.PatientId,
                NurseId = nurseId,
                SupplementName = request.SupplementName,
                Quantity = request.Quantity,
                DosingHours = request.DosingHours,
                DurationDays = request.DurationDays
            };

            var command = StartTreatmentCommandFromResourceAssembler.ToCommand(resource);
            var result = await _facade.StartTreatmentAsync(command);

            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            // 🔍 LOG DEL ERROR COMPLETO
            Console.WriteLine($"Error en StartTreatment: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message, detail = ex.ToString() });
        }
    }
    
    // ==========================================
    // 2. CONFIRMAR DOSIS - SOLO MOTHER
    // ==========================================

    [HttpPost("doses/confirm")]
    [RequireRole("Mother")]
    public async Task<IActionResult> ConfirmDose([FromBody] ConfirmDoseRequest request)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return Unauthorized(new { error = "Mother ID no encontrado en token" });
            }

            var command = new ConfirmDoseCommand(request.PatientId, motherId);
            var result = await _facade.ConfirmDoseAsync(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 3. COMPLETAR TRATAMIENTO - SOLO NURSE
    // ==========================================

    [HttpPut("treatments/complete")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> CompleteTreatment([FromBody] CompleteTreatmentRequest request)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return Unauthorized(new { error = "Nurse ID no encontrado en el token" });
            }

            var resource = new CompleteTreatmentResource
            {
                TreatmentId = request.TreatmentId,
                NurseId = nurseId,
                Observation = request.Observation
            };

            var command = CompleteTreatmentCommandFromResourceAssembler.ToCommand(resource);
            var result = await _facade.CompleteTreatmentAsync(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 4. ABANDONAR TRATAMIENTO - SOLO NURSE
    // ==========================================

    [HttpPut("treatments/abandon")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> AbandonTreatment([FromBody] AbandonTreatmentRequest request)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return Unauthorized(new { error = "Nurse ID no encontrado en el token" });
            }

            var resource = new AbandonTreatmentResource
            {
                TreatmentId = request.TreatmentId,
                NurseId = nurseId,
                Observation = request.Observation
            };

            var command = AbandonTreatmentCommandFromResourceAssembler.ToCommand(resource);
            var result = await _facade.AbandonTreatmentAsync(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 5. EVALUAR DOSIS OMITIDA - PUBLICO (PRUEBA)
    // ==========================================

    [HttpPost("doses/evaluate-missed")]
    [AllowAnonymous]
    public async Task<IActionResult> EvaluateMissedDose([FromBody] EvaluateMissedDoseRequest request)
    {
        try
        {
            var resource = new EvaluateMissedDoseResource
            {
                DailyDoseId = request.DailyDoseId
            };

            var command = EvaluateMissedDoseCommandFromResourceAssembler.ToCommand(resource);
            var result = await _facade.EvaluateMissedDoseAsync(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 6. OBTENER DOSIS DE HOY - SOLO MOTHER
    // ==========================================

    [HttpGet("patients/{patientId}/today-dose")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetTodayDose(string patientId)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return Unauthorized(new { error = "Mother ID not found in token" });
            }

            var query = new GetTodayDoseQuery(patientId, motherId);
            var result = await _facade.GetTodayDoseAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 7. OBTENER HISTORIAL DE DOSIS - SOLO MOTHER
    // ==========================================

    [HttpGet("patients/{patientId}/dose-history")]
    [RequireRole("Mother")]
    public async Task<IActionResult> GetPatientDoseHistory(string patientId)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return BadRequest(new { error = "Mother ID not found in token" });
            }

            var query = new GetPatientDoseHistoryQuery(patientId);
            var result = await _facade.GetPatientDoseHistoryAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 8. OBTENER PACIENTES PENDIENTES - SOLO NURSE
    // ==========================================

    [HttpGet("nurses/pending-patients")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetPendingPatientsByNurse()
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID not found in token" });
            }

            var query = new GetPendingPatientsByNurseQuery(nurseId);
            var result = await _facade.GetPendingPatientsByNurseAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 9. OBTENER VISTA GENERAL DE RIESGO - SOLO NURSE
    // ==========================================

    [HttpGet("risk-overview")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetRiskLevelOverview()
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID not found in token" });
            }

            var query = new GetRiskLevelOverviewQuery(nurseId);
            var result = await _facade.GetRiskLevelOverviewAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 10. OBTENER TRATAMIENTOS POR ENFERMERA - SOLO NURSE
    // ==========================================

    [HttpGet("nurses/treatments")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetTreatmentsByNurse([FromQuery] string? status = null)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID not found in token" });
            }

            TreatmentStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status))
            {
                statusEnum = TreatmentStatusExtensions.FromString(status);
            }

            var query = new GetTreatmentsByNurseQuery(nurseId, statusEnum);
            var result = await _facade.GetTreatmentsByNurseAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 11. OBTENER DETALLES DE TRATAMIENTO - SOLO NURSE
    // ==========================================

    [HttpGet("treatments/{treatmentId}")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetTreatmentDetails(string treatmentId)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return Unauthorized(new { error = "Nurse ID not found in token" });
            }

            var query = new GetTreatmentDetailsQuery(treatmentId);
            var result = await _facade.GetTreatmentDetailsAsync(query);

            // Validar que el paciente está asignado a esta enfermera
            var treatmentData = result.GetType().GetProperty("patientId")?.GetValue(result)?.ToString();
            if (!string.IsNullOrEmpty(treatmentData))
            {
                await _facade.ValidateNurseHasPatientAsync(nurseId, treatmentData);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 12. OBTENER PACIENTES POR NIVEL DE RIESGO - SOLO NURSE
    // ==========================================

    [HttpGet("risk/{riskLevel}/patients")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetPatientsByRiskLevel(string riskLevel)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return BadRequest(new { error = "Nurse ID not found in token" });
            }

            var riskLevelEnum = RiskLevelExtensions.FromString(riskLevel);
            var query = new GetPatientsByRiskLevelQuery(riskLevelEnum, nurseId);
            var result = await _facade.GetPatientsByRiskLevelAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 13. OBTENER DETALLE DE TRATAMIENTO DE PACIENTE - SOLO NURSE
    // ==========================================

    [HttpGet("patients/{patientId}/treatment-detail")]
    [RequireRole("Nurse")]
    public async Task<IActionResult> GetPatientTreatmentDetail(string patientId)
    {
        try
        {
            var nurseId = User.FindFirst("nurseId")?.Value;

            if (string.IsNullOrEmpty(nurseId))
            {
                return Unauthorized(new { error = "Nurse ID not found in token" });
            }

            await _facade.ValidateNurseHasPatientAsync(nurseId, patientId);

            var query = new GetPatientTreatmentDetailQuery(patientId);
            var result = await _facade.GetPatientTreatmentDetailAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 14. MÉTODOS DE PRUEBA (FORCE)
    // ==========================================

    [HttpPost("testing/force-omit-dose")]
    [AllowAnonymous]
    public async Task<IActionResult> ForceOmitDoseForTesting([FromBody] ForceDoseRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DailyDoseId))
            {
                return BadRequest(new { error = "dailyDoseId is required" });
            }

            var result = await _facade.ForceOmitDoseForTestingAsync(request.DailyDoseId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("testing/force-confirm-dose")]
    [AllowAnonymous]
    public async Task<IActionResult> ForceConfirmDoseForTesting([FromBody] ForceDoseRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DailyDoseId))
            {
                return BadRequest(new { error = "dailyDoseId is required" });
            }

            var result = await _facade.ForceConfirmDoseForTestingAsync(request.DailyDoseId);
            return Ok(result);
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

public class StartTreatmentRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string SupplementName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string DosingHours { get; set; } = string.Empty;
    public int DurationDays { get; set; }
}

public class ConfirmDoseRequest
{
    public string PatientId { get; set; } = string.Empty;
}

public class CompleteTreatmentRequest
{
    public string TreatmentId { get; set; } = string.Empty;
    public string? Observation { get; set; }
}

public class AbandonTreatmentRequest
{
    public string TreatmentId { get; set; } = string.Empty;
    public string? Observation { get; set; }
}

public class EvaluateMissedDoseRequest
{
    public string DailyDoseId { get; set; } = string.Empty;
}

public class ForceDoseRequest
{
    public string DailyDoseId { get; set; } = string.Empty;
}