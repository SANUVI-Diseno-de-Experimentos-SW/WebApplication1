using WebApplication1.iam.infrastructure.Email;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.iam.infrastructure.tokens;
using WebApplication1.iam.Interfaces.Facades;

namespace WebApplication1.iam.Interfaces.DependencyInjection;

using WebApplication1.Contexts.IAM.Application.Interfaces.OutboundServices;
using WebApplication1.Contexts.IAM.Application.Services;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.Contexts.IAM.Domain.Services;
using WebApplication1.Contexts.IAM.Infrastructure.Security;


/// <summary>
/// Configuración de inyección de dependencias para el módulo IAM
/// </summary>
public static class UserDependencyInjection
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        // ==========================================
        // 📌 REPOSITORIOS (Infrastructure)
        // ==========================================
        services.AddScoped<IUserRepository, MongoUserRepository>();

        // ==========================================
        // 📌 SERVICIOS DE INFRAESTRUCTURA
        // ==========================================
        services.AddScoped<IBcryptHashingService, BcryptHashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ==========================================
        // 📌 SERVICIOS DE EMAIL
        // ==========================================
        services.AddHttpClient<IEmailService, ResendEmailService>();

        // ==========================================
        // 📌 SERVICIOS DE APLICACIÓN
        // ==========================================
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IUserQueryService, UserQueryService>();

        // ==========================================
        // 📌 FACADE (Anticorrupción Layer)
        // ==========================================
        services.AddScoped<UserFacade>();

        // ==========================================
        // 📌 CONTROLLERS
        // ==========================================
        // Los controllers se registran automáticamente con AddControllers()

        return services;
    }
}