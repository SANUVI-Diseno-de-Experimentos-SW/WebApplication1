using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.patient_management.Application.Internal;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Domain.Services;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;
using WebApplication1.patient_management.Interfaces.Facades;
using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;

namespace WebApplication1.patient_management.Interfaces.DependencyInjection;



public static class PatientManagementDependencyInjection
{
    public static IServiceCollection AddPatientManagementServices(this IServiceCollection services)
    {
        // ==========================================
        // REPOSITORIOS
        // ==========================================
        
        services.AddScoped<IPatientRepository, MongoPatientRepository>();
        services.AddScoped<IMedicalRecordRepository, MongoMedicalRecordRepository>();
        
        // Repositorios de otros bounded contexts (se implementarán después)
        // Por ahora usamos implementaciones dummy o null
        services.AddScoped<INurseAssignmentRepository, MongoNurseAssignmentRepository>();
        services.AddScoped<ITreatmentRepository, MongoTreatmentRepository>();
        services.AddScoped<IDailyDoseRepository, MongoDailyDoseRepository>();
        services.AddScoped<IAchievementRepository, MongoAchievementRepository>();
        services.AddScoped<IBadgeRepository, MongoBadgeRepository>();
        services.AddScoped<IConsultationRepository, MongoConsultationRepository>();
        
        // Repositorio de usuarios de IAM
        services.AddScoped<IUserRepository, MongoUserRepository>();

        // ==========================================
        // SERVICIOS DE APLICACIÓN
        // ==========================================
        
        services.AddScoped<IPatientCommandService, PatientCommandServiceImpl>();
        services.AddScoped<IPatientQueryService, PatientQueryServiceImpl>();

        // ==========================================
        // FACADE
        // ==========================================
        
        services.AddScoped<PatientManagementFacade>();

        // ==========================================
        // CONTROLLERS
        // ==========================================
        
        services.AddScoped<PatientManagementController>();

        return services;
    }
}
