
using WebApplication1.AnalyticsReporting.Application.Dtos;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Domain.Repositories;

namespace WebApplication1.AnalyticsReporting.Infrastructure.Persitencia.MongoDb.Repositories;

public class MongoAnalyticsRepository
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly INurseAssignmentRepository _nurseAssignmentRepository;
    private readonly IHealthFacilityRepository _healthFacilityRepository;
    private readonly ILogger<MongoAnalyticsRepository> _logger;

    public MongoAnalyticsRepository(
        ITreatmentRepository treatmentRepository,
        INurseAssignmentRepository nurseAssignmentRepository,
        IHealthFacilityRepository healthFacilityRepository,
        ILogger<MongoAnalyticsRepository> logger)
    {
        _treatmentRepository = treatmentRepository;
        _nurseAssignmentRepository = nurseAssignmentRepository;
        _healthFacilityRepository = healthFacilityRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryResponseDto> GetDashboardSummaryAsync()
    {
        var facilitiesAnalytics = await ComputeFacilitiesAnalyticsAsync();

        var totalActiveFacilities = facilitiesAnalytics.Count;
        var totalCriticalFacilities = facilitiesAnalytics.Count(f => f.RiskLevel == "HIGH");

        var globalAdherenceRate = facilitiesAnalytics.Count > 0
            ? facilitiesAnalytics.Average(f => f.AdherenceRate)
            : 0;

        // ✅ Usar el constructor del DTO original
        return new DashboardSummaryResponseDto(
            totalActiveFacilities,
            totalCriticalFacilities,
            Math.Round(globalAdherenceRate, 2)
        );
    }

    public async Task<FacilitiesAnalyticsResponseDto> GetFacilitiesAnalyticsAsync(string? riskLevelFilter = null)
    {
        var facilities = await ComputeFacilitiesAnalyticsAsync();

        if (!string.IsNullOrEmpty(riskLevelFilter))
        {
            facilities = facilities.Where(f => f.RiskLevel == riskLevelFilter).ToList();
        }

        // ✅ Convertir RiskLevel a string con el formato correcto
        var items = facilities.Select(f => new FacilityAnalyticsItemDto(
            f.FacilityId,
            f.FacilityName,
            f.DistrictName,
            f.AdherenceRate,
            f.RiskLevel,
            f.TotalPatients,
            f.TotalConfirmed,
            f.TotalOmitted
        )).ToList();

        // ✅ Usar el constructor del DTO original
        return new FacilitiesAnalyticsResponseDto(items);
    }

    public async Task<HeatmapDataResponseDto> GetFacilityHeatmapDataAsync(string? riskLevelFilter = null)
    {
        var facilities = await ComputeFacilitiesAnalyticsAsync();

        if (!string.IsNullOrEmpty(riskLevelFilter))
        {
            facilities = facilities.Where(f => f.RiskLevel == riskLevelFilter).ToList();
        }

        // ✅ Usar el constructor del DTO original
        var points = facilities.Select(f => new HeatmapPointDto(
            f.FacilityId,
            f.FacilityName,
            f.Lat,
            f.Lng,
            f.RiskLevel,
            f.AdherenceRate
        )).ToList();

        return new HeatmapDataResponseDto(points);
    }

    public async Task<TopFacilitiesResponseDto> GetTopFacilitiesAsync(int limit = 4)
    {
        var facilities = await ComputeFacilitiesAnalyticsAsync();

        var topFacilities = facilities
            .OrderByDescending(f => f.AdherenceRate)
            .Take(limit)
            .ToList();

        // ✅ Convertir a FacilityAnalyticsItemDto usando el constructor
        var items = topFacilities.Select(f => new FacilityAnalyticsItemDto(
            f.FacilityId,
            f.FacilityName,
            f.DistrictName,
            f.AdherenceRate,
            f.RiskLevel,
            f.TotalPatients,
            f.TotalConfirmed,
            f.TotalOmitted
        )).ToList();

        // ✅ Usar el constructor del DTO original
        return new TopFacilitiesResponseDto(items);
    }

    private async Task<List<FacilityAnalyticsData>> ComputeFacilitiesAnalyticsAsync()
    {
        var activeTreatments = await _treatmentRepository.FindAllActiveAsync();

        var facilityMap = new Dictionary<string, FacilityAnalyticsData>();

        foreach (var treatment in activeTreatments)
        {
            var nurseId = treatment.NurseId;
            var assignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(nurseId);

            if (assignment == null) continue;

            var facilityId = assignment.FacilityId;
            var facility = await _healthFacilityRepository.FindByIdAsync(facilityId);

            if (facility == null) continue;

            if (!facilityMap.ContainsKey(facilityId))
            {
                var facilityData = facility.ToPrimitives();
                facilityMap[facilityId] = new FacilityAnalyticsData
                {
                    FacilityId = facilityId,
                    FacilityName = facilityData.Name,
                    DistrictName = facilityData.DistrictName,
                    Lat = facilityData.Coordinates.Lat,
                    Lng = facilityData.Coordinates.Lng,
                    TotalConfirmed = 0,
                    TotalOmitted = 0,
                    TotalPatients = 0,
                    RiskLevels = new List<string>()
                };
            }

            var entry = facilityMap[facilityId];
            var treatmentData = treatment.ToPrimitives();
            entry.TotalConfirmed += treatmentData.TotalConfirmed;
            entry.TotalOmitted += treatmentData.TotalOmitted;
            entry.TotalPatients++;
            entry.RiskLevels.Add(treatment.RiskScore.RiskLevel.ToStringValue());
        }

        var result = new List<FacilityAnalyticsData>();

        foreach (var (_, data) in facilityMap)
        {
            var totalDoses = data.TotalConfirmed + data.TotalOmitted;
            var adherenceRate = totalDoses == 0 ? 100 : (double)data.TotalConfirmed / totalDoses * 100;

            var riskLevel = data.RiskLevels.Contains("HIGH") ? "HIGH" :
                            data.RiskLevels.Contains("MEDIUM") ? "MEDIUM" : "LOW";

            result.Add(new FacilityAnalyticsData
            {
                FacilityId = data.FacilityId,
                FacilityName = data.FacilityName,
                DistrictName = data.DistrictName,
                Lat = data.Lat,
                Lng = data.Lng,
                AdherenceRate = Math.Round(adherenceRate, 2),
                RiskLevel = riskLevel,
                TotalPatients = data.TotalPatients,
                TotalConfirmed = data.TotalConfirmed,
                TotalOmitted = data.TotalOmitted
            });
        }

        return result.OrderByDescending(f => f.AdherenceRate).ToList();
    }

    private class FacilityAnalyticsData
    {
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double AdherenceRate { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public int TotalPatients { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalOmitted { get; set; }
        public List<string> RiskLevels { get; set; } = new();
    }
}