using System.Collections.Concurrent;

namespace DRMA.WEB.Core
{
    /// <summary>
    /// Buffers calls to an event until a subscriber is registered, then replays them automatically.
    /// </summary>
    public static class BufferedEvent
    {
        private static readonly ConcurrentDictionary<string, Func<object?[], Task>> Handlers = new();
        private static readonly ConcurrentDictionary<string, object?[]> PendingEvents = new();

        public static void Register(string name, Func<object?[], Task> handler)
        {
            Handlers[name] = handler;

            if (PendingEvents.TryRemove(name, out var args))
            {
                _ = SafeInvoke(handler, args);
            }
        }

        public static void Register<T>(string name, Func<T, Task> handler)
        {
            Register(name, args => handler((T)args[0]!));
        }

        public static void Register(string name, Func<Task> handler)
        {
            Register(name, _ => handler());
        }

        public static Task Invoke(string name, params object?[] args)
        {
            if (Handlers.TryGetValue(name, out var handler))
            {
                return SafeInvoke(handler, args);
            }

            PendingEvents[name] = args;
            return Task.CompletedTask;
        }

        private static async Task SafeInvoke(Func<object?[], Task> handler, object?[] args)
        {
            await handler(args);
        }
    }
}