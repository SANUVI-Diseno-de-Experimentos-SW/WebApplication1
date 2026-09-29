namespace WebApplication1.HealthyFacility.Domain.Models.Queries;

public record ListHealthFacilitiesQuery(
    double UserLatitude,
    double UserLongitude,
    string MotherId
);