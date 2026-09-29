namespace WebApplication1.HealthyFacility.Domain.Models.Queries;

public record GetMyTopAppointmentsQuery(
    string NurseId,
    int? Limit = 4
);