using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DRMA.API.Core;

public sealed class CosmosLoggerProvider(CosmosLogRepository repo) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CosmosLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new CosmosLogger(name, repo));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class CosmosLogger(string name, CosmosLogRepository repo) : ILogger
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

        _ = repo.Add(new LogModel
        {
            Name = name,
            State = formatter(state, exception),
            Message = exception?.Message,
            StackTrace = exception?.StackTrace,
            Ttl = (int)TtlCache.ThreeMonths
        });
    }
}
