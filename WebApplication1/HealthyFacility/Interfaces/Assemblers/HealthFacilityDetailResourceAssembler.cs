using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Interfaces.Resources;

namespace WebApplication1.HealthyFacility.Interfaces.Assemblers;


public static class HealthFacilityDetailResourceAssembler
{
    public static HealthFacilityDetailResource ToResource(HealthFacility facility)
    {
        var data = facility.ToPrimitives();

        return new HealthFacilityDetailResource
        {
            Name = data.Name,
            Address = data.Address,
            DistrictName = data.DistrictName,
            PhoneNumber = data.PhoneNumber,
            Services = data.Services,
            AvailableDays = data.OperatingSchedule.AvailableDays,
            AvailableSlots = data.OperatingSchedule.AvailableSlots,
            ScheduleOfOperation = data.ScheduleOfOperation,
            Status = data.Status
        };
    }
}