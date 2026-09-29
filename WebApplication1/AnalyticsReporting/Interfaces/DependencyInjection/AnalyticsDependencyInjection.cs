using WebApplication1.AnalyticsReporting.Application.Internal;
using WebApplication1.AnalyticsReporting.Domain.Services;
using WebApplication1.AnalyticsReporting.Infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;

namespace WebApplication1.AnalyticsReporting.Interfaces.DependencyInjection;

public static class AnalyticsDependencyInjection
{
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS DE OTROS BCs
        // ==========================================

        services.AddScoped<ITreatmentRepository, MongoTreatmentRepository>();
        services.AddScoped<INurseAssignmentRepository, MongoNurseAssignmentRepository>();
        services.AddScoped<IHealthFacilityRepository, MongoHealthFacilityRepository>();

        // ==========================================
        // REPOSITORIO DE ANALYTICS
        // ==========================================

        services.AddScoped<MongoAnalyticsRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryServiceImpl>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<AnalyticsController>();

        return services;
    }
}