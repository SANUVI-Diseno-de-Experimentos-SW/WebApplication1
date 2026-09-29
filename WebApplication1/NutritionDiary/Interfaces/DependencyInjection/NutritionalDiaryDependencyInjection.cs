using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.NutritionDiary.Application.Internal;
using WebApplication1.NutritionDiary.Domain.Repositories;
using WebApplication1.NutritionDiary.Domain.Services;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Repositories;
using WebApplication1.NutritionDiary.Interfaces.Facade;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;

namespace WebApplication1.NutritionDiary.Interfaces.DependencyInjection;



public static class NutritionalDiaryDependencyInjection
{
    public static IServiceCollection AddNutritionalDiaryServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================

        services.AddScoped<INutritionalDiaryRepository, MongoNutritionalDiaryRepository>();
        services.AddScoped<IFoodEntryRepository, MongoFoodEntryRepository>();
        services.AddScoped<IFoodItemRepository, MongoFoodItemRepository>();

        // Repositorios externos
        services.AddScoped<IPatientRepository, MongoPatientRepository>();
        services.AddScoped<IUserRepository, MongoUserRepository>();

        // ==========================================
        // SERVICIOS DE DOMINIO
        // ==========================================

        services.AddScoped<IIronCalculatorService, IronCalculatorServiceImpl>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<INutritionalDiaryCommandService, NutritionalDiaryCommandServiceImpl>();
        services.AddScoped<INutritionalDiaryQueryService, NutritionalDiaryQueryServiceImpl>();

        // ==========================================
        // FACADE
        // ==========================================

        services.AddScoped<NutritionalDiaryFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<NutritionalDiaryController>();

        return services;
    }
}