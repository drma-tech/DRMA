using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DRMA.API.Core
{
    public sealed class CosmosLoggerProvider(CosmosLogRepository repo) : ILoggerProvider
    {
        private readonly CosmosLogRepository _repo = repo;
        private readonly ConcurrentDictionary<string, CosmosLogger> _loggers = new();

        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new CosmosLogger(name, _repo));

        public void Dispose() => _loggers.Clear();
    }

    public class CosmosLogger(string name, CosmosLogRepository repo) : ILogger
    {
        private readonly string _name = name;
        private readonly CosmosLogRepository _repo = repo;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (exception is NotificationException)
            {
                return;
            }

            _ = _repo.Add(new LogModel()
            {
                Name = _name,
                State = formatter(state, exception),
                Message = exception?.Message,
                StackTrace = exception?.StackTrace,
                Ttl = (int)ttlCache.three_months
            });
        }
    }
}