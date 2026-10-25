using System.Text.Json;

namespace WebApplication1.Tests.shared.Mongo;

/// <summary>
/// Lee propiedades de un JsonElement sin importar mayusculas/minusculas del nombre
/// y aceptando varios nombres posibles (por ejemplo "patientId" o "id").
/// </summary>
public static class TestJson
{
    public static JsonElement? Find(this JsonElement json, params string[] names)
    {
        if (json.ValueKind != JsonValueKind.Object) return null;
 
        foreach (var property in json.EnumerateObject())
        {
            if (names.Any(n => string.Equals(n, property.Name, StringComparison.OrdinalIgnoreCase)))
                return property.Value;
        }
 
        return null;
    }
 
    public static string? FindString(this JsonElement json, params string[] names)
    {
        var value = json.Find(names);
        if (value is null) return null;
 
        return value.Value.ValueKind == JsonValueKind.String
            ? value.Value.GetString()
            : value.Value.ToString();
    }
}
