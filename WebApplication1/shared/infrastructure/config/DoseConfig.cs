using System;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.shared.infrastructure.config;

/// <summary>
/// Configuración para el umbral de omisión de dosis
/// </summary>
public static class DoseConfig
{
    private static IConfiguration? _configuration;

    /// <summary>
    /// Inicializa la configuración (llamar en Program.cs)
    /// </summary>
    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Propiedad estática (para compatibilidad con código existente)
    /// Este valor se calcula UNA SOLA VEZ al iniciar la aplicación
    /// </summary>
    public static double OmissionThresholdHours { get; } = CalculateThreshold();

    /// <summary>
    /// Método dinámico para obtener el umbral (RECOMENDADO)
    /// Este método se ejecuta CADA VEZ que se necesita el valor
    /// Permite cambios en tiempo real si se modifica la variable de entorno
    /// </summary>
    public static double GetOmissionThresholdHours()
    {
        // Obtener el entorno actual
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                  ?? "Production";

        Console.WriteLine($"[DoseConfig] Entorno detectado: {env}");

        // ==============================================
        // LÓGICA PARA DESARROLLO
        // ==============================================
        if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) || 
            env.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            // Intentar leer desde appsettings primero
            var devThreshold = GetDevOmissionMinutesFromConfig();
            
            // Si no está en appsettings, intentar desde variable de entorno del sistema
            if (string.IsNullOrEmpty(devThreshold))
            {
                devThreshold = Environment.GetEnvironmentVariable("DEV_OMISSION_MINUTES");
            }

            Console.WriteLine($"[DoseConfig] DEV_OMISSION_MINUTES: {devThreshold ?? "NO DEFINIDA"}");

            // Si el usuario configuró un valor personalizado
            if (!string.IsNullOrEmpty(devThreshold) && double.TryParse(devThreshold, out var threshold))
            {
                // Convertir minutos → horas (porque el sistema trabaja en horas)
                var hours = threshold / 60;
                Console.WriteLine($"[DoseConfig] Umbral configurado: {hours} horas ({threshold} minutos)");
                return hours;
            }

            // Si NO hay variable, usar 1 minuto por defecto
            Console.WriteLine("[DoseConfig] Usando valor por defecto: 1 minuto");
            return 1.0 / 60; // 1 minuto en desarrollo
        }

        // ==============================================
        // LÓGICA PARA PRODUCCIÓN
        // ==============================================
        Console.WriteLine("[DoseConfig] Usando valor de producción: 24 horas");
        return 24; // 24 horas en producción
    }

    private static string? GetDevOmissionMinutesFromConfig()
    {
        try
        {
            if (_configuration == null) return null;
            
            // Intentar obtener desde la sección EnvironmentVariables
            var value = _configuration["EnvironmentVariables:DEV_OMISSION_MINUTES"];
            if (!string.IsNullOrEmpty(value)) return value;
            
            // Intentar obtener directamente
            return _configuration["DEV_OMISSION_MINUTES"];
        }
        catch
        {
            return null;
        }
    }

    private static double CalculateThreshold()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                  ?? "Production";

        if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) || 
            env.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            var devThreshold = GetDevOmissionMinutesFromConfig();
            
            if (string.IsNullOrEmpty(devThreshold))
            {
                devThreshold = Environment.GetEnvironmentVariable("DEV_OMISSION_MINUTES");
            }

            if (!string.IsNullOrEmpty(devThreshold) && double.TryParse(devThreshold, out var threshold))
            {
                return threshold / 60;
            }

            return 1.0 / 60;
        }

        return 24;
    }
}