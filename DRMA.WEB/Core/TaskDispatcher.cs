namespace DRMA.WEB.Core
{
    public sealed class TaskDispatcher<T>
    {
        private readonly Lock _sync = new();
        private readonly List<Func<T, Task>> _handlers = [];

        public void Subscribe(Func<T, Task> handler, CancellationToken token)
        {
            lock (_sync)
            {
                _handlers.Add(handler);
            }

            token.Register(() =>
            {
                lock (_sync)
                {
                    _handlers.Remove(handler);
                }
            });
        }

        public async Task PublishAsync(T value)
        {
            Func<T, Task>[] snapshot;

            lock (_sync)
            {
                snapshot = [.. _handlers];
            }

            foreach (var handler in snapshot)
            {
                try
                {
                    await handler(value);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation.
                }
            }
        }
    }

    public sealed class TaskDispatcher
    {
        private readonly Lock _sync = new();
        private readonly List<Func<Task>> _handlers = [];

        public void Subscribe(Func<Task> handler, CancellationToken token)
        {
            lock (_sync)
            {
                _handlers.Add(handler);
            }

            token.Register(() =>
            {
                lock (_sync)
                {
                    _handlers.Remove(handler);
                }
            });
        }

        public async Task PublishAsync()
        {
            Func<Task>[] snapshot;

            lock (_sync)
            {
                snapshot = [.. _handlers];
            }

            foreach (var handler in snapshot)
            {
                try
                {
                    await handler();
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation.
                }
            }
        }
    }
}
