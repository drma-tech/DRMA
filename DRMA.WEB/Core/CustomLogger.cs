using System.Collections.Concurrent;

namespace DRMA.WEB.Core;

public sealed class CosmosLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new CustomLogger(name));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class CustomLogger(string name) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        if (exception is NotificationException) return;

        AppStateStatic.Logs.Add(new LogContainer
        {
            Name = name,
            State = formatter(state, exception),
            Message = exception?.Message,
            StackTrace = exception?.StackTrace
        });
    }
}

public class LogContainer
{
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}