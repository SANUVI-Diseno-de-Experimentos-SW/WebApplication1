namespace WebApplication1.Tests.shared.Mongo;

/// <summary>
/// Todas las clases de tests marcadas con [Collection(ApiCollection.Name)] comparten
/// UNA sola factory (una sola API y un solo Mongo2Go) y se ejecutan una tras otra.
/// Esto evita levantar varias APIs en el mismo proceso: tu Program.cs registra
/// serializadores globales de Mongo y fallaria si se ejecutara dos veces.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<MongoWebApplicationFactory>
{
    public const string Name = "Api";
}
