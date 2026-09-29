using System;

namespace WebApplication1.Shared.Infrastructure.Config;

/// <summary>
/// Configuración de entorno de la aplicación
/// </summary>
public static class Env
{
    /// <summary>
    /// Puerto donde corre la aplicación
    /// </summary>
    public static int Port { get; } = GetPort();

    /// <summary>
    /// Entorno actual (Development, Test, Production)
    /// </summary>
    public static string Environment { get; } = 
        System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
        ?? "Production";

    /// <summary>
    /// Obtiene la cadena de conexión a MongoDB
    /// </summary>
    public static string MongoUri { get; } = 
        System.Environment.GetEnvironmentVariable("MONGO_URI") 
        ?? "mongodb://localhost:27017/ferova";

    private static int GetPort()
    {
        var portEnv = System.Environment.GetEnvironmentVariable("PORT");
        return int.TryParse(portEnv, out var port) ? port : 5000;
    }
}