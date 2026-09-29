namespace WebApplication1.HealthyFacility.Application.DTOs;

public class HealthFacilityAdminItemDto
{
    public string Id { get; }
    public string Name { get; }
    public string Address { get; }
    public string? AssignedNurseName { get; }
    public bool HasNurseAssigned { get; }
    public string? DisplayMessage { get; }

    public HealthFacilityAdminItemDto(
        string id,
        string name,
        string address,
        string? assignedNurseName,
        bool hasNurseAssigned,
        string? displayMessage = null)
    {
        Id = id;
        Name = name;
        Address = address;
        AssignedNurseName = assignedNurseName;
        HasNurseAssigned = hasNurseAssigned;
        DisplayMessage = displayMessage;
    }
}

public class HealthFacilityAdminListResponseDto
{
    public int Total { get; }
    public List<HealthFacilityAdminItemDto> HealthFacilities { get; }

    public HealthFacilityAdminListResponseDto(int total, List<HealthFacilityAdminItemDto> healthFacilities)
    {
        Total = total;
        HealthFacilities = healthFacilities;
    }
}