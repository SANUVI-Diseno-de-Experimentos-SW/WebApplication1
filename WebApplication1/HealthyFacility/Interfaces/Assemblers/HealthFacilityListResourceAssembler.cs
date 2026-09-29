using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Interfaces.Resources;

namespace WebApplication1.HealthyFacility.Interfaces.Assemblers;



public static class HealthFacilityListResourceAssembler
{
    public static HealthFacilityListResource ToResource(HealthFacility facility, double distanceKm)
    {
        var data = facility.ToPrimitives();

        return new HealthFacilityListResource
        {
            Id = data.Id,
            Name = data.Name,
            Status = data.Status,
            DistanceKm = distanceKm
        };
    }
}