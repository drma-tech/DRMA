using System.Collections.Concurrent;

namespace DRMA.WEB.Core.Helper
{
    public class TaskHelper
    {
        private readonly ConcurrentDictionary<string, Lazy<Task>> _running = new();

        public Task RunSingleAsync(string key, Func<Task> factory)
        {
            var lazy = _running.GetOrAdd(key, k =>
                new Lazy<Task>(() => Wrap(k, factory), LazyThreadSafetyMode.ExecutionAndPublication)
            );

            return lazy.Value;
        }

        private async Task Wrap(string key, Func<Task> factory)
        {
            try
            {
                await factory();
            }
            finally
            {
                _running.TryRemove(key, out _);
            }
        }
    }
}