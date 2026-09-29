namespace WebApplication1.AnalyticsReporting.Application.Dtos;

using System.Collections.Generic;

public class TopFacilitiesResponseDto
{
    public List<FacilityAnalyticsItemDto> Facilities { get; }

    public TopFacilitiesResponseDto(List<FacilityAnalyticsItemDto> facilities)
    {
        Facilities = facilities;
    }
}