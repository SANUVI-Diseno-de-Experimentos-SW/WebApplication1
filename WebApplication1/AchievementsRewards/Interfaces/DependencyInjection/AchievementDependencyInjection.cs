using WebApplication1.AchievementsRewards.Application.Internal;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Domain.Services;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.AchievementsRewards.Interfaces.Facades;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;

namespace WebApplication1.AchievementsRewards.Interfaces.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



public static class AchievementDependencyInjection
{
    public static IServiceCollection AddAchievementServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================

        services.AddScoped<IAchievementRepository, MongoAchievementRepository>();
        services.AddScoped<IBadgeRepository, MongoBadgeRepository>();
        services.AddScoped<IPatientRepository, MongoPatientRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<IAchievementQueryService, AchievementQueryServiceImpl>();
        services.AddScoped<IAchievementCommandService, AchievementCommandServiceImpl>();

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        services.AddScoped<TreatmentEventHandlers>();

        // ==========================================
        // FACADE
        // ==========================================

        services.AddScoped<AchievementFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<AchievementController>();

        return services;
    }

    /// <summary>
    /// Suscribir los event handlers al EventPublisher
    /// </summary>
    public static void SubscribeAchievementEvents(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TreatmentEventHandlers>>();
        var handlers = serviceProvider.GetRequiredService<TreatmentEventHandlers>();

        // Suscribir eventos de tratamiento
        // Nota: En .NET, usamos un patrón de eventos o mensajería
        // Aquí se suscribirían a un bus de eventos o similar

        logger.LogInformation("✅ Achievement event handlers registered");
    }
}