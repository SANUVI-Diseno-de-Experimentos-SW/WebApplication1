namespace WebApplication1.patient_management.Domain.Model.DTos;

// Nuevo archivo: Domain/DTOs/HemoglobinControlsHistoryDto.cs

public class HemoglobinControlsHistoryDto
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public List<ControlItemDto> Controls { get; set; } = new();
    public double? AverageHemoglobin { get; set; }
    public int TotalControls { get; set; }
    public double? Evolution { get; set; }
    public string? Trend { get; set; }
}

public class ControlItemDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double? HemoglobinLevel { get; set; }
    public string AnemiaStatus { get; set; } = string.Empty;
}