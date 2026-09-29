using WebApplication1.Consultation.Application.Internal;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Domain.Servicies;
using WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.Consultation.Interfaces.Facades;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;

namespace WebApplication1.Consultation.Interfaces.DependencyInjection;




public static class CommunicationDependencyInjection
{
    public static IServiceCollection AddCommunicationServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================

        services.AddScoped<IConsultationRepository, MongoConsultationRepository>();
        services.AddScoped<IPatientRepository, MongoPatientRepository>();
        services.AddScoped<IUserRepository, MongoUserRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================

        services.AddScoped<ICommunicationCommandService, CommunicationCommandServiceImpl>();
        services.AddScoped<ICommunicationQueryService, CommunicationQueryServiceImpl>();

        // ==========================================
        // FACADE
        // ==========================================

        services.AddScoped<CommunicationFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================

        services.AddScoped<CommunicationController>();

        return services;
    }
}