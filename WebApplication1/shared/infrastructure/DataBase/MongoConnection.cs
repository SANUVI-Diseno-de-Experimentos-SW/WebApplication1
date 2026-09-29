namespace WebApplication1.shared.infrastructure.DataBase;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;


/// <summary>
/// Conexión a MongoDB
/// </summary>
public static class MongoConnection
{
    private static IMongoDatabase? _database;
    private static readonly object _lock = new();

    /// <summary>
    /// Configura MongoDB en el contenedor de DI
    /// </summary>
    public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar serializadores
        ConfigureSerializers();

        // Obtener la cadena de conexión
        var connectionString = configuration.GetValue<string>("MONGO_URI") 
                               ?? "mongodb://localhost:27017/ferova";

        // Crear el cliente MongoDB
        var client = new MongoClient(connectionString);
        var databaseName = new MongoUrl(connectionString).DatabaseName ?? "ferova";
        _database = client.GetDatabase(databaseName);

        // Registrar el IMongoDatabase en el contenedor
        services.AddSingleton(_database);

        // Registrar el cliente
        services.AddSingleton(client);

        Console.WriteLine("MongoDB configured successfully");

        return services;
    }

    /// <summary>
    /// Configura serializadores personalizados
    /// </summary>
    private static void ConfigureSerializers()
    {
        // Serializar ObjectId como string
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        
        // Configurar DateTime para usar UTC
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));
    }

    /// <summary>
    /// Obtiene la instancia de la base de datos (para uso directo)
    /// </summary>
    public static IMongoDatabase GetDatabase()
    {
        if (_database == null)
        {
            throw new InvalidOperationException("MongoDB not initialized. Call AddMongoDB first.");
        }
        return _database;
    }

    /// <summary>
    /// Obtiene una colección de MongoDB
    /// </summary>
    public static IMongoCollection<T> GetCollection<T>(string? collectionName = null)
    {
        var database = GetDatabase();
        var name = collectionName ?? typeof(T).Name.ToLowerInvariant() + "s";
        return database.GetCollection<T>(name);
    }
}