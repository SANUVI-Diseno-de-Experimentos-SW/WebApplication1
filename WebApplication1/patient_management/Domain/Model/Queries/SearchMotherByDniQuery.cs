namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para buscar una madre por DNI
/// </summary>
public record SearchMotherByDniQuery(
    string SearchTerm
);