// EventPublisher.cs
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebApplication1.shared.infrastructure.Events;

public class EventPublisher
{
    private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _handlers = new();
    private readonly ILogger<EventPublisher>? _logger;

    public EventPublisher()
    {
    }

    // Constructor con logger para poder ver logs
    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(string eventName, Func<TEvent, Task> handler) where TEvent : class
    {
        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Func<object, Task>>();
        }

        Func<object, Task> genericHandler = async (eventObj) =>
        {
            try
            {
                _logger?.LogInformation("[EventPublisher] Handler recibido para {EventName}", eventName);

                if (eventObj is TEvent typedEvent)
                {
                    await handler(typedEvent);
                    _logger?.LogInformation("[EventPublisher] Handler ejecutado correctamente para {EventName}", eventName);
                }
                else
                {
                    // 🔥 NUEVO: Intentar convertir objeto anónimo a tipo usando reflexión
                    _logger?.LogWarning("[EventPublisher] Evento no es del tipo esperado. Intentando mapear...");
                    
                    var mapped = MapToType<TEvent>(eventObj);
                    if (mapped != null)
                    {
                        _logger?.LogInformation("[EventPublisher] Evento mapeado correctamente");
                        await handler(mapped);
                    }
                    else
                    {
                        _logger?.LogError("[EventPublisher] No se pudo mapear el evento {EventName}", eventName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[EventPublisher] Error en handler para {EventName}", eventName);
                Console.WriteLine($"[EventPublisher] Error: {ex.Message}");
                Console.WriteLine($"[EventPublisher] StackTrace: {ex.StackTrace}");
            }
        };

        _handlers[eventName].Add(genericHandler);
        Console.WriteLine($"[EventPublisher] ✅ Handler suscrito a: {eventName}");
        _logger?.LogInformation("[EventPublisher] Handler suscrito a: {EventName}", eventName);
    }

    // 🔥 NUEVO MÉTODO: Mapear objeto anónimo a tipo específico
    private T? MapToType<T>(object source) where T : class
    {
        try
        {
            var target = Activator.CreateInstance<T>();
            var sourceProps = source.GetType().GetProperties();
            var targetProps = typeof(T).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                var targetProp = targetProps.FirstOrDefault(p => 
                    string.Equals(p.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase) && p.CanWrite);
                
                if (targetProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    
                    // Manejar conversiones de tipos
                    if (value != null && targetProp.PropertyType != sourceProp.PropertyType)
                    {
                        try
                        {
                            value = Convert.ChangeType(value, targetProp.PropertyType);
                        }
                        catch
                        {
                            _logger?.LogWarning("[EventPublisher] No se pudo convertir {PropName} de {SourceType} a {TargetType}", 
                                sourceProp.Name, sourceProp.PropertyType.Name, targetProp.PropertyType.Name);
                            continue;
                        }
                    }
                    
                    targetProp.SetValue(target, value);
                }
            }

            return target;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[EventPublisher] Error mapeando objeto a {TargetType}", typeof(T).Name);
            return null;
        }
    }

    public async Task PublishAsync<TEvent>(string eventName, TEvent @event)
    {
        Console.WriteLine($"[EventPublisher] 📤 Publicando evento: {eventName}");
        _logger?.LogInformation("[EventPublisher] Publicando evento: {EventName}", eventName);

        if (!_handlers.TryGetValue(eventName, out var handlers) || handlers.Count == 0)
        {
            Console.WriteLine($"[EventPublisher] ⚠️ No hay handlers para: {eventName}");
            _logger?.LogWarning("[EventPublisher] No hay handlers para: {EventName}", eventName);
            return;
        }

        Console.WriteLine($"[EventPublisher] 📤 {handlers.Count} handler(s) encontrados para {eventName}");
        
        foreach (var handler in handlers)
        {
            try
            {
                await handler(@event!);
                Console.WriteLine($"[EventPublisher] ✅ Handler ejecutado para {eventName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventPublisher] ❌ Error en handler: {ex.Message}");
                _logger?.LogError(ex, "[EventPublisher] Error en handler para {EventName}", eventName);
            }
        }
    }

    public async Task PublishAsync(string eventName, object @event)
    {
        Console.WriteLine($"[EventPublisher] 📤 Publicando evento (sin tipo): {eventName}");
        _logger?.LogInformation("[EventPublisher] Publicando evento (sin tipo): {EventName}", eventName);

        if (!_handlers.TryGetValue(eventName, out var handlers) || handlers.Count == 0)
        {
            Console.WriteLine($"[EventPublisher] ⚠️ No hay handlers para: {eventName}");
            _logger?.LogWarning("[EventPublisher] No hay handlers para: {EventName}", eventName);
            return;
        }

        Console.WriteLine($"[EventPublisher] 📤 {handlers.Count} handler(s) encontrados para {eventName}");
        
        foreach (var handler in handlers)
        {
            try
            {
                await handler(@event);
                Console.WriteLine($"[EventPublisher] ✅ Handler ejecutado para {eventName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventPublisher] ❌ Error en handler: {ex.Message}");
                _logger?.LogError(ex, "[EventPublisher] Error en handler para {EventName}", eventName);
            }
        }
    }
}