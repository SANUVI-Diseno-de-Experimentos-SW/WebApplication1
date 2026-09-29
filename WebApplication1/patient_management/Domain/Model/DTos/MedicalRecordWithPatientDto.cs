namespace WebApplication1.patient_management.Domain.Model.DTos;



public class MedicalRecordWithPatientDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;  // ✅ NUEVO
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public double? HemoglobinLevel { get; set; }
    public double Weight { get; set; }
    public double Height { get; set; }
    public string Gender { get; set; } = string.Empty;
    public List<AntecedenteDto> Antecedentes { get; set; } = new();
    public string MotivoConsulta { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public List<string> Sintomas { get; set; } = new();
    public List<ControlItemDto> Controls { get; set; } = new();
    public string? NurseId { get; set; }
}

public class AntecedenteDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

