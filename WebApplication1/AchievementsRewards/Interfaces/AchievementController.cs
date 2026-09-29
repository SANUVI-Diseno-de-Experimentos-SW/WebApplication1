using WebApplication1.AchievementsRewards.Domain.Model.Queries;
using WebApplication1.AchievementsRewards.Interfaces.Facades;
using WebApplication1.shared.Attributes;

namespace WebApplication1.AchievementsRewards.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/achievements-rewards")]
[Authorize]
public class AchievementController : ControllerBase
{
    private readonly AchievementFacade _facade;

    public AchievementController(AchievementFacade facade)
    {
        _facade = facade;
    }

    /// <summary>
    /// GET /patients/{patientId}/achievement
    /// Obtiene el progreso de un paciente (tarjeta principal)
    /// </summary>
    [HttpGet("patients/{patientId}/achievement")]
    [Authorize]
    [RequireRole("Mother")] 
    public async Task<IActionResult> GetPatientAchievement(string patientId)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return Unauthorized(new { error = "Mother ID not found in token" });
            }

            var query = new GetPatientAchievementQuery(patientId, motherId);
            var result = await _facade.GetPatientAchievementAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /patients/{patientId}/badges
    /// Obtiene todas las medallas de un paciente
    /// </summary>
    [HttpGet("patients/{patientId}/badges")]
    [Authorize]
    [RequireRole("Mother")] 
    public async Task<IActionResult> GetPatientBadges(string patientId)
    {
        try
        {
            var motherId = User.FindFirst("motherId")?.Value;

            if (string.IsNullOrEmpty(motherId))
            {
                return Unauthorized(new { error = "Mother ID not found in token" });
            }

            var query = new GetPatientBadgesQuery(patientId, motherId);
            var result = await _facade.GetPatientBadgesAsync(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// [SOLO PRUEBAS] Fuerza la evaluación de badges para un paciente
    /// </summary>
    [HttpPost("force-evaluate/{patientId}")]
    [AllowAnonymous] // Solo para pruebas
    public async Task<IActionResult> ForceEvaluateBadges(string patientId)
    {
        try
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest(new { error = "patientId is required" });
            }

            var result = await _facade.ForceEvaluateBadgesAsync(patientId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}