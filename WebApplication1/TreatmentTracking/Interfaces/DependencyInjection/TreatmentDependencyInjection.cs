using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;
using WebApplication1.shared.infrastructure.Events;
using WebApplication1.TreatmentTracking.Application.Internal.Services;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Services;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.TreatmentTracking.Interfaces.Facades;

namespace WebApplication1.TreatmentTracking.Interfaces.DependencyInjection;

public static class TreatmentDependencyInjection
{
    public static IServiceCollection AddTreatmentServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================

        services.AddScoped<ITreatmentRepository, MongoTreatmentRepository>();
        services.AddScoped<IDailyDoseRepository, MongoDailyDoseRepository>();
        services.AddScoped<IPatientRepository, MongoPatientRepository>();
        services.AddScoped<IAchievementRepository, MongoAchievementRepository>();
        services.AddScoped<IBadgeRepository, MongoBadgeRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<ITreatmentCommandService, TreatmentCommandServiceImpl>();
        services.AddScoped<ITreatmentQueryService, TreatmentQueryServiceImpl>();

        // ==========================================
        // FACADE
        // ==========================================

        services.AddScoped<TreatmentFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<TreatmentController>();

        // ==========================================
        // EVENT PUBLISHER (SINGLETON)
        // ==========================================

        services.AddSingleton<EventPublisher>();

        return services;
    }
}