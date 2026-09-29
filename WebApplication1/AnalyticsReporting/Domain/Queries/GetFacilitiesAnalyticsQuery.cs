namespace WebApplication1.AnalyticsReporting.Domain.Queries;

public record GetFacilitiesAnalyticsQuery(
    string? RiskLevelFilter = null  // "LOW", "MEDIUM", "HIGH"
);