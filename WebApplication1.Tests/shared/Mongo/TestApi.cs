using System.Net;
using System.Net.Http.Json;

namespace WebApplication1.Tests.shared.Mongo;
/// <summary>
/// Utilidades genericas para los tests de integracion de CUALQUIER contexto.
/// Aqui no van URLs ni datos de un contexto en particular.
/// </summary>
public static class TestApi
{
    /// <summary>Envia un POST con JSON y limpia los errores capturados de la API antes de enviarlo.</summary>
    public static Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body)
    {
        ErrorCapturingLoggerProvider.Errors.Clear();
        return client.PostAsJsonAsync(url, body);
    }
 
    /// <summary>
    /// Verifica el codigo HTTP. Si no coincide, el mensaje del test incluye el cuerpo de la
    /// respuesta y las excepciones que la API registro, para no tener que adivinar el 500.
    /// </summary>
    public static async Task AssertStatusAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        if (response.StatusCode == expected) return;
 
        var body = await response.Content.ReadAsStringAsync();
        var errors = string.Join(Environment.NewLine, ErrorCapturingLoggerProvider.Errors);
 
        Assert.Fail(
            $"Esperado: {(int)expected} {expected}. Recibido: {(int)response.StatusCode} {response.StatusCode}." +
            $"{Environment.NewLine}Body: {body}" +
            $"{Environment.NewLine}Errores de la API:{Environment.NewLine}{errors}");
    }
}