namespace DRMA.WEB.Core
{
    public sealed class ActionDispatcher<T>
    {
#pragma warning disable MA0046 // Use EventHandler<T> to declare events

        private event Action<T>? Handlers;

#pragma warning restore MA0046 // Use EventHandler<T> to declare events

        public void Subscribe(Action<T> handler, CancellationToken token)
        {
            Handlers += handler;

            token.Register(() => { Handlers -= handler; });
        }

        public void Publish(T value)
        {
            Handlers?.Invoke(value);
        }
    }

    public sealed class ActionDispatcher
    {
#pragma warning disable MA0046 // Use EventHandler<T> to declare events

        private event Action? Handlers;

#pragma warning restore MA0046 // Use EventHandler<T> to declare events

        public void Subscribe(Action handler, CancellationToken token)
        {
            Handlers += handler;

            token.Register(() => { Handlers -= handler; });
        }

        public void Publish()
        {
            Handlers?.Invoke();
        }
    }
}
