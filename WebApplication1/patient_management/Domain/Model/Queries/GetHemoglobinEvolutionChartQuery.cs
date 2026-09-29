namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener datos para el gráfico de evolución de hemoglobina
/// </summary>
public record GetHemoglobinEvolutionChartQuery(
    string PatientId
);