namespace WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class ControlDocument
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double? HemoglobinLevel { get; set; }
    public string? AnemiaStatus { get; set; }
}

public class AntecedenteDocument
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class MedicalRecordDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string MedicalRecordId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string? NurseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public double? HemoglobinLevel { get; set; }
    public double Weight { get; set; }
    public double Height { get; set; }
    public string Gender { get; set; } = string.Empty;
    public List<AntecedenteDocument> Antecedentes { get; set; } = new();
    public string MotivoConsulta { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<string> Sintomas { get; set; } = new();
    public List<ControlDocument> Controls { get; set; } = new();
}