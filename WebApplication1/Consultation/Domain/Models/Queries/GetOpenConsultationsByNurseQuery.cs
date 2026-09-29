namespace WebApplication1.Consultation.Domain.Models.Queries;

/// <summary>
/// SearchTerm: para buscar una consulta abierta en base al nombre del paciente o la madre
/// </summary>
public record GetOpenConsultationsByNurseQuery(
    string NurseId,
    string? SearchTerm = null
);