using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.HealthyFacility.Application.Services;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Services;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;
using WebApplication1.HealthyFacility.Interfaces.Facades;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;
using WebApplication1.shared.catalogs.Data;

namespace WebApplication1.HealthyFacility.Interfaces.DependencyInjection;



public static class HealthFacilityDependencyInjection
{
    public static IServiceCollection AddHealthFacilityServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================

        services.AddScoped<IHealthFacilityRepository, MongoHealthFacilityRepository>();
        services.AddScoped<IAppointmentRepository, MongoAppointmentRepository>();
        services.AddScoped<INurseAssignmentRepository, MongoNurseAssignmentRepository>();
        services.AddSingleton<DistrictRepository>();

        // Repositorios externos
        services.AddScoped<IUserRepository, MongoUserRepository>();
        services.AddScoped<IPatientRepository, MongoPatientRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<IHealthyFacilityCommandService, HealthFacilityCommandServiceImpl>();
        services.AddScoped<IHealthyFacilityQueryService, HealthFacilityQueryServiceImpl>();

        // ==========================================
        // FACADE
        // ==========================================

        services.AddScoped<HealthFacilityFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<HealthFacilityController>();

        return services;
    }
}