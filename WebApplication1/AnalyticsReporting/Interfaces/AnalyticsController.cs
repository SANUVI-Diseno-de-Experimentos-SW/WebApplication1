using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.AnalyticsReporting.Domain.Queries;
using WebApplication1.AnalyticsReporting.Domain.Services;
using WebApplication1.shared.Attributes;

namespace WebApplication1.AnalyticsReporting.Interfaces;


[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsQueryService _analyticsQueryService;

    public AnalyticsController(IAnalyticsQueryService analyticsQueryService)
    {
        _analyticsQueryService = analyticsQueryService;
    }

    // ==========================================
    // 1. DASHBOARD SUMMARY - SOLO ADMIN
    // ==========================================

    [HttpGet("dashboard/summary")]
    [RequireRole("Admin")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var result = await _analyticsQueryService.GetDashboardSummaryAsync(new GetDashboardSummaryQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 2. FACILITIES ANALYTICS - SOLO ADMIN
    // ==========================================

    [HttpGet("facilities")]
    [RequireRole("Admin")]
    public async Task<IActionResult> GetFacilitiesAnalytics([FromQuery] string? riskLevel = null)
    {
        try
        {
            var result = await _analyticsQueryService.GetFacilitiesAnalyticsAsync(
                new GetFacilitiesAnalyticsQuery(riskLevel)
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 3. HEATMAP DATA - SOLO ADMIN
    // ==========================================

    [HttpGet("heatmap")]
    [RequireRole("Admin")]
    public async Task<IActionResult> GetFacilityHeatmapData([FromQuery] string? riskLevel = null)
    {
        try
        {
            var result = await _analyticsQueryService.GetFacilityHeatmapDataAsync(
                new GetFacilityHeatmapDataQuery(riskLevel)
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 4. TOP FACILITIES - SOLO ADMIN
    // ==========================================

    [HttpGet("facilities/top")]
    [RequireRole("Admin")]
    public async Task<IActionResult> GetTopFacilities()
    {
        try
        {
            var result = await _analyticsQueryService.GetTopFacilitiesAsync(new GetTopFacilitiesQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // 5. GENERATE PDF REPORT - SOLO ADMIN
    // ==========================================

    [HttpGet("report/pdf")]
    [RequireRole("Admin")]
    public async Task<IActionResult> GeneratePdfReport()
    {
        try
        {
            var result = await _analyticsQueryService.GeneratePdfReportAsync(new GeneratePdfReportQuery());

            // Convertir base64 a buffer
            var pdfBuffer = Convert.FromBase64String(result.PdfBase64);

            // Configurar headers para descarga
            Response.Headers.Add("Content-Type", "application/pdf");
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{result.FileName}\"");
            Response.Headers.Add("Content-Length", pdfBuffer.Length.ToString());

            // Enviar el buffer directamente
            return File(pdfBuffer, "application/pdf", result.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}