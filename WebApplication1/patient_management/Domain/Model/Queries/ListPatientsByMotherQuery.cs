namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para listar pacientes por madre
/// </summary>
public record ListPatientsByMotherQuery(
    string MotherId
);