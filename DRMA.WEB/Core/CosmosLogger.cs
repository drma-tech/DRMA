using DRMA.WEB.Modules.Subscription.Core;
using System.Collections.Concurrent;

namespace DRMA.WEB.Core;

public sealed class CosmosLoggerProvider(LoggerApi api) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CosmosLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new CosmosLogger(api));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class CosmosLogger(LoggerApi api) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Error;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        if (exception is NotificationException) return;

        var log = new LogModel
        {
            Message = exception?.Message ?? formatter(state, exception),
            StackTrace = exception?.StackTrace,
            Origin = "DRMA - Blazor",
            Params = null,
            Body = null,
            OperationSystem = null,
            BrowserName = null,
            BrowserVersion = null,
            Platform = AppStateStatic.GetSavedPlatform().ToString(),
            AppVersion = AppStateStatic.Version,
            //UserId = AppStateStatic.UserId,
            Ip = null,
            UserAgent = null,
            Ttl = (int)TtlCache.ThreeMonths
        };

        _ = api.SaveLog(log);
    }
}