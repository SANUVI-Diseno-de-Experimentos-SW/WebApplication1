using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using WebApplication1.AchievementsRewards.Application.Internal;
using WebApplication1.AchievementsRewards.Domain.Repositories;
using WebApplication1.AchievementsRewards.Domain.Services;
using WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.AchievementsRewards.Interfaces.Facades;
using WebApplication1.AnalyticsReporting.Interfaces.DependencyInjection;
using WebApplication1.Consultation.Application.Internal;
using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Domain.Servicies;
using WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Repository;
using WebApplication1.Consultation.Interfaces.Facades;
using WebApplication1.Contexts.IAM.Application.Interfaces.OutboundServices;
using WebApplication1.Contexts.IAM.Application.Services;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.Contexts.IAM.Domain.Services;
using WebApplication1.Contexts.IAM.Infrastructure.Security;
using WebApplication1.HealthyFacility.Application.Services;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Services;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Repositories;
using WebApplication1.HealthyFacility.Interfaces.Facades;
using WebApplication1.iam.infrastructure.Email;
using WebApplication1.iam.infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.iam.infrastructure.tokens;
using WebApplication1.iam.Interfaces.Facades;
using WebApplication1.NutritionDiary.Interfaces.DependencyInjection;
using WebApplication1.patient_management.Application.Internal;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.patient_management.Domain.Services;
using WebApplication1.patient_management.Infrastructure.Persitencia.MongoDb.Repositoreis;
using WebApplication1.patient_management.Interfaces.Facades;
using WebApplication1.Services;
using WebApplication1.shared.catalogs.Data;
using WebApplication1.shared.infrastructure.config;
using WebApplication1.shared.infrastructure.Events;
using WebApplication1.TreatmentTracking.Application.Internal.Scheduling;
using WebApplication1.TreatmentTracking.Application.Internal.Services;
using WebApplication1.TreatmentTracking.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Services;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Repositories;
using WebApplication1.TreatmentTracking.Interfaces.Facades;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURAR SERIALIZADORES DE MONGODB
// ==========================================
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

// ==========================================
// 2. CONFIGURAR MONGODB
// ==========================================
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var mongoConnectionString = mongoSettings["ConnectionString"] 
                            ?? "mongodb://localhost:27017";
var mongoDatabaseName = mongoSettings["DatabaseName"] 
                        ?? "ferova";

builder.Services.AddScoped<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// ==========================================
// ✅ 3. REGISTRAR EVENT PUBLISHER (SINGLETON)
// ==========================================
builder.Services.AddSingleton<EventPublisher>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<EventPublisher>>();
    return new EventPublisher(logger);
});
// ==========================================
// ✅ 3. CONFIGURAR AUTENTICACIÓN JWT
// ==========================================

// Obtener la clave secreta JWT
var jwtSecret = builder.Configuration["JWT_SECRET"];

// Asegurar clave mínima de 32 caracteres
if (jwtSecret.Length < 32)
{
    jwtSecret = jwtSecret.PadRight(32, '!');
}

var key = Encoding.UTF8.GetBytes(jwtSecret);

// ✅ REGISTRAR AUTENTICACIÓN
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ==========================================
// 1. REGISTRAR SERVICIOS IAM (Identity & Access Management)
// ==========================================

// 📌 Repositorios (Infrastructure)
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();

// 📌 Servicios de Infraestructura
builder.Services.AddScoped<IBcryptHashingService, BcryptHashingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 📌 Servicios de Email
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();

// 📌 Servicios de Aplicación
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// 📌 Facade
builder.Services.AddScoped<UserFacade>();

// 📌 Hosted Services
builder.Services.AddSingleton<DistrictRepository>();
builder.Services.AddHostedService<DatabaseSeeder>();

// ==========================================
// 2. REGISTRAR SERVICIOS ACHIEVEMENTS & REWARDS
// ==========================================

// 📌 Repositorios Achievements
builder.Services.AddScoped<IAchievementRepository, MongoAchievementRepository>();
builder.Services.AddScoped<IBadgeRepository, MongoBadgeRepository>();

// 📌 Servicios de Aplicación Achievements
builder.Services.AddScoped<IAchievementQueryService, AchievementQueryServiceImpl>();
builder.Services.AddScoped<IAchievementCommandService, AchievementCommandServiceImpl>();

// 📌 Event Handlers
builder.Services.AddScoped<TreatmentEventHandlers>();

// 📌 Facade Achievements
builder.Services.AddScoped<AchievementFacade>();

// ==========================================
// 3. REGISTRAR SERVICIOS COMMUNICATION MANAGEMENT
// ==========================================

// 📌 Repositorios Communication
builder.Services.AddScoped<IConsultationRepository, MongoConsultationRepository>();

// 📌 Servicios de Aplicación Communication
builder.Services.AddScoped<ICommunicationCommandService, CommunicationCommandServiceImpl>();
builder.Services.AddScoped<ICommunicationQueryService, CommunicationQueryServiceImpl>();

// 📌 Facade Communication
builder.Services.AddScoped<CommunicationFacade>();

// ==========================================
// 6. REGISTRAR SERVICIOS TREATMENT TRACKING
// ==========================================

// 📌 Repositorios Treatment Tracking
builder.Services.AddScoped<ITreatmentRepository, MongoTreatmentRepository>();
builder.Services.AddScoped<IDailyDoseRepository, MongoDailyDoseRepository>();

// 📌 Servicios de Aplicación Treatment Tracking
builder.Services.AddScoped<ITreatmentCommandService, TreatmentCommandServiceImpl>();
builder.Services.AddScoped<ITreatmentQueryService, TreatmentQueryServiceImpl>();

// 📌 Facade Treatment Tracking
builder.Services.AddScoped<TreatmentFacade>();

builder.Services.AddHostedService<DoseEvaluationScheduler>();


// ==========================================
// 7. REGISTRAR SERVICIOS PATIENT MANAGEMENT
// ==========================================

// 📌 Repositorios Patient Management
builder.Services.AddScoped<IPatientRepository, MongoPatientRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MongoMedicalRecordRepository>();

// 📌 Servicios de Aplicación Patient Management
builder.Services.AddScoped<IPatientCommandService, PatientCommandServiceImpl>();
builder.Services.AddScoped<IPatientQueryService, PatientQueryServiceImpl>();

// 📌 Facade Patient Management
builder.Services.AddScoped<PatientManagementFacade>();

// ==========================================
// 8. REGISTRAR SERVICIOS HEALTHY FACILITY
// ==========================================

// 📌 Repositorios Healthy Facility
builder.Services.AddScoped<IHealthFacilityRepository, MongoHealthFacilityRepository>();
builder.Services.AddScoped<IAppointmentRepository, MongoAppointmentRepository>();
builder.Services.AddScoped<INurseAssignmentRepository, MongoNurseAssignmentRepository>();

// 📌 Servicios de Aplicación Healthy Facility
builder.Services.AddScoped<IHealthyFacilityCommandService, HealthFacilityCommandServiceImpl>();
builder.Services.AddScoped<IHealthyFacilityQueryService, HealthFacilityQueryServiceImpl>();

// 📌 Facade Healthy Facility
builder.Services.AddScoped<HealthFacilityFacade>();

// ==========================================
// ✅ 10. REGISTRAR SERVICIOS NUTRITION DIARY
// ==========================================

builder.Services.AddNutritionalDiaryServices();

// ==========================================
// 11 REGISTRAR SERVICIOS ANALYTICS & REPORTING
// ==========================================

builder.Services.AddAnalyticsServices();


// ==========================================
//  CONFIGURAR CONTROLLERS
// ==========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ==========================================
//  CONFIGURAR SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ferova API",
        Version = "v1",
        Description = "Healthcare management API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese el token JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
             },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ==========================================
//  CONFIGURAR CORS
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ==========================================
//  CONSTRUIR APLICACIÓN
// ==========================================
var app = builder.Build();

// ==========================================
// ✅ INICIALIZAR DOSECONFIG CON LA CONFIGURACIÓN
// ==========================================
DoseConfig.Initialize(builder.Configuration);

// ==========================================
//  CONFIGURAR MIDDLEWARES - IMPORTANTE: ORDEN
// ==========================================

// ✅ Swagger - DEBE IR ANTES de UseRouting
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ferova API V1");
        c.RoutePrefix = "api-docs"; // ✅ Esto hace que Swagger esté en /api-docs
        // c.RoutePrefix = string.Empty; // ✅ Si quieres que esté en la raíz
    });
}

// ✅ HTTPS (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ✅ CORS
app.UseCors("AllowAll");

// ✅ Routing
app.UseRouting(); // ⚠️ IMPORTANTE: Debe estar antes de UseAuthentication/UseAuthorization

// ✅ Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// ✅ Mapear Controladores
app.MapControllers();

// ==========================================
// ✅ SUSCRIBIR EVENT HANDLERS DE ACHIEVEMENTS
// ==========================================

// Program.cs - En la sección de suscripción de eventos
using (var scope = app.Services.CreateScope())
{
    var eventPublisher = scope.ServiceProvider.GetRequiredService<EventPublisher>();
    var handlers = scope.ServiceProvider.GetRequiredService<TreatmentEventHandlers>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TreatmentEventHandlers>>();

    Console.WriteLine("==========================================");
    Console.WriteLine("📢 SUSCRIBIENDO EVENT HANDLERS");
    Console.WriteLine("==========================================");

    // 1. Treatment Started
    Console.WriteLine("[EventPublisher] Suscribiendo TreatmentStarted...");
    eventPublisher.Subscribe<TreatmentStartedEvent>("TreatmentStarted", async (eventData) =>
    {
        try
        {
            Console.WriteLine($"[EventPublisher] 🔥 TreatmentStarted DISPARADO para: {eventData.TreatmentId}");
            await handlers.OnTreatmentStartedAsync(eventData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventPublisher] ❌ Error en TreatmentStarted: {ex.Message}");
            Console.WriteLine($"[EventPublisher] StackTrace: {ex.StackTrace}");
            logger.LogError(ex, "Error en TreatmentStarted handler");
        }
    });
    Console.WriteLine("[EventPublisher] ✅ TreatmentStarted suscrito");

    // 2. Daily Dose Confirmed
    Console.WriteLine("[EventPublisher] Suscribiendo DailyDoseConfirmed...");
    eventPublisher.Subscribe<DailyDoseConfirmedEvent>("DailyDoseConfirmed", async (eventData) =>
    {
        try
        {
            Console.WriteLine($"[EventPublisher] 🔥 DailyDoseConfirmed DISPARADO para: {eventData.TreatmentId}");
            await handlers.OnDailyDoseConfirmedAsync(eventData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventPublisher] ❌ Error en DailyDoseConfirmed: {ex.Message}");
            logger.LogError(ex, "Error en DailyDoseConfirmed handler");
        }
    });
    Console.WriteLine("[EventPublisher] ✅ DailyDoseConfirmed suscrito");

    // 3. Daily Dose Omitted
    Console.WriteLine("[EventPublisher] Suscribiendo DailyDoseOmitted...");
    eventPublisher.Subscribe<DailyDoseOmittedEvent>("DailyDoseOmitted", async (eventData) =>
    {
        try
        {
            Console.WriteLine($"[EventPublisher] 🔥 DailyDoseOmitted DISPARADO para: {eventData.TreatmentId}");
            await handlers.OnDailyDoseOmittedAsync(eventData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventPublisher] ❌ Error en DailyDoseOmitted: {ex.Message}");
            logger.LogError(ex, "Error en DailyDoseOmitted handler");
        }
    });
    Console.WriteLine("[EventPublisher] ✅ DailyDoseOmitted suscrito");

    // 4. Treatment Completed
    Console.WriteLine("[EventPublisher] Suscribiendo TreatmentCompleted...");
    eventPublisher.Subscribe<TreatmentCompletedEvent>("TreatmentCompleted", async (eventData) =>
    {
        try
        {
            Console.WriteLine($"[EventPublisher] 🔥 TreatmentCompleted DISPARADO para: {eventData.TreatmentId}");
            await handlers.OnTreatmentCompletedAsync(eventData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventPublisher] ❌ Error en TreatmentCompleted: {ex.Message}");
            logger.LogError(ex, "Error en TreatmentCompleted handler");
        }
    });
    Console.WriteLine("[EventPublisher] ✅ TreatmentCompleted suscrito");

    // 5. Treatment Abandoned
    Console.WriteLine("[EventPublisher] Suscribiendo TreatmentAbandoned...");
    eventPublisher.Subscribe<TreatmentAbandonedEvent>("TreatmentAbandoned", async (eventData) =>
    {
        try
        {
            Console.WriteLine($"[EventPublisher] 🔥 TreatmentAbandoned DISPARADO para: {eventData.TreatmentId}");
            await handlers.OnTreatmentAbandonedAsync(eventData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventPublisher] ❌ Error en TreatmentAbandoned: {ex.Message}");
            logger.LogError(ex, "Error en TreatmentAbandoned handler");
        }
    });
    Console.WriteLine("[EventPublisher] ✅ TreatmentAbandoned suscrito");

    Console.WriteLine("==========================================");
    Console.WriteLine("✅ ALL EVENT HANDLERS REGISTERED");
    Console.WriteLine("==========================================");
}
// ==========================================
// 9. INICIAR APLICACIÓN
// ==========================================
var port = builder.Configuration["PORT"] ?? "5002";
Console.WriteLine($"🚀 Aplicación iniciada en puerto: {port}");
Console.WriteLine($"📚 Swagger UI: http://localhost:{port}/api-docs");
Console.WriteLine($"📚 Swagger JSON: http://localhost:{port}/swagger/v1/swagger.json");
Console.WriteLine($"🔗 API Base: http://localhost:{port}/api/users");

app.Run();