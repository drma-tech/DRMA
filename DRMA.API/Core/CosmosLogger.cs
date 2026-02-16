using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DRMA.API.Core;

public sealed class CosmosLoggerProvider(CosmosLogRepository repo) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CosmosLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new CosmosLogger(repo));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class CosmosLogger(CosmosLogRepository repo) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        if (exception is NotificationException) return;

        var kvs = state as IEnumerable<KeyValuePair<string, object>>;
        var context = kvs?.FirstOrDefault().Value as LogModel;

        var log = new LogModel
        {
            Message = context?.Message ?? exception?.Message ?? formatter(state, exception),
            InnerException = context?.InnerException ?? exception?.InnerException?.Message,
            StackTrace = context?.StackTrace ?? exception?.StackTrace,
            Origin = context?.Origin,
            Params = context?.Params,
            Body = context?.Body,
            OperationSystem = context?.OperationSystem,
            BrowserName = context?.BrowserName,
            BrowserVersion = context?.BrowserVersion,
            Platform = context?.Platform,
            AppVersion = context?.AppVersion,
            UserId = context?.UserId,
            Ip = context?.Ip,
            Country = context?.Country,
            UserAgent = context?.UserAgent,
            IsBot = context?.IsBot,
            Ttl = (int)TtlCache.ThreeMonths
        };

        _ = repo.Add(log);
    }
}
