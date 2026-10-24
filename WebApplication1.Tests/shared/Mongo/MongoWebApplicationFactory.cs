using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Mongo2Go;

namespace WebApplication1.Tests.shared.Mongo;

// ==========================================
// FACTORY: levanta la API completa + MongoDB en memoria
// (equivale a @SpringBootTest + Flapdoodle Embedded MongoDB)
// ==========================================
public class MongoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MongoDbRunner _mongo = MongoDbRunner.Start();
 
    public MongoWebApplicationFactory()
    {
        // Tu Program.cs lee Mongo y JWT_SECRET ANTES de builder.Build(), por eso
        // ConfigureAppConfiguration llega tarde. Las variables de entorno si se leen a tiempo.
        // "__" equivale a ":" (MongoDB__ConnectionString = MongoDB:ConnectionString).
        Environment.SetEnvironmentVariable("MongoDB__ConnectionString", _mongo.ConnectionString);
        Environment.SetEnvironmentVariable("MongoDB__DatabaseName", "webapplication1_tests");
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-for-integration-tests-0123456789");
    }
 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.AddProvider(new ErrorCapturingLoggerProvider()));
    }
 
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _mongo.Dispose();
    }
}