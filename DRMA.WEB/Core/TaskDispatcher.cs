namespace DRMA.WEB.Core
{
    public sealed class TaskDispatcher<T>
    {
        private readonly List<Func<T, Task>> handlers = [];

        public void Subscribe(Func<T, Task> handler, CancellationToken token)
        {
            handlers.Add(handler);

            token.Register(() => { handlers.Remove(handler); });
        }

        public async Task PublishAsync(T value)
        {
            var snapshot = handlers.ToArray();

            foreach (var handler in snapshot)
            {
                await handler(value);
            }
        }
    }

    public sealed class TaskDispatcher
    {
        private readonly List<Func<Task>> handlers = [];

        public void Subscribe(Func<Task> handler, CancellationToken token)
        {
            handlers.Add(handler);

            token.Register(() => { handlers.Remove(handler); });
        }

        public async Task PublishAsync()
        {
            var snapshot = handlers.ToArray();

            foreach (var handler in snapshot)
            {
                await handler();
            }
        }
    }
}