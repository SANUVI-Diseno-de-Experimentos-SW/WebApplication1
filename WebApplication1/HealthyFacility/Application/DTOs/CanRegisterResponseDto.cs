namespace WebApplication1.HealthyFacility.Application.DTOs;

public class CanRegisterResponseDto
{
    public bool Available { get; }
    public string Message { get; }
    public string? Details { get; }

    public CanRegisterResponseDto(bool available, string message, string? details = null)
    {
        Available = available;
        Message = message;
        Details = details;
    }
}