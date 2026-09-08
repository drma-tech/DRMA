namespace SD.WEB.Core.Helper
{
    /// <summary>
    /// Ensures that only one task is active for each key and context.
    /// A new context cancels the previous task and starts a new one.
    /// </summary>
    public sealed class TaskHelper
    {
        private sealed class State
        {
            public object? Context { get; init; }
            public CancellationTokenSource CancellationTokenSource { get; init; } = default!;
            public Task Task { get; set; } = Task.CompletedTask;
        }

        private readonly Lock _sync = new();
        private readonly Dictionary<string, State> _states = new(StringComparer.OrdinalIgnoreCase);

        public Task RunSingleAsync<TContext>(string key, TContext context, Func<CancellationToken, Task> factory, CancellationToken externalToken)
        {
            lock (_sync)
            {
                if (_states.TryGetValue(key, out var existing))
                {
                    if (Equals(existing.Context, context))
                        return existing.Task;

                    existing.CancellationTokenSource.Cancel();
                }

                var internalCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);

                var state = new State
                {
                    Context = context,
                    CancellationTokenSource = internalCts,
                };

                _states[key] = state;

                state.Task = ExecuteAsync(state, linkedCts);

                return state.Task;
            }

            async Task ExecuteAsync(State state, CancellationTokenSource linkedCts)
            {
                try
                {
                    await factory(linkedCts.Token);
                }
                finally
                {
                    linkedCts.Dispose();
                    state.CancellationTokenSource.Dispose();

                    lock (_sync)
                    {
                        if (_states.TryGetValue(key, out var current) && ReferenceEquals(current, state))
                        {
                            _states.Remove(key);
                        }
                    }
                }
            }
        }
    }
}
