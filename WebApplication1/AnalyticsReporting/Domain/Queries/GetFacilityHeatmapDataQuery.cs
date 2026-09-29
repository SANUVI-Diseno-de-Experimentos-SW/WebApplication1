namespace WebApplication1.AnalyticsReporting.Domain.Queries;

public record GetFacilityHeatmapDataQuery(
    string? RiskLevelFilter = null  // "LOW", "MEDIUM", "HIGH"
);