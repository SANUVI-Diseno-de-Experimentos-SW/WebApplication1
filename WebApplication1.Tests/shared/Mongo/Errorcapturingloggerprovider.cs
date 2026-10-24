using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Tests.shared.Mongo;

/// <summary>
/// Guarda en memoria los logs de nivel Error de la API, para poder mostrarlos
/// en el mensaje del Assert (el controller oculta la excepcion real en el 500).
/// </summary>
public sealed class ErrorCapturingLoggerProvider : ILoggerProvider
{
    public static readonly ConcurrentQueue<string> Errors = new();
 
    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName);
 
    public void Dispose() { }
 
    private sealed class CapturingLogger(string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
 
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
 
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
 
            var text = formatter(state, exception);
            if (exception is not null) text += Environment.NewLine + exception;
 
            Errors.Enqueue($"[{category}] {text}");
        }
    }
}
