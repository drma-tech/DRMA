using System.Collections.Concurrent;

namespace DRMA.WEB.Core.Helper
{
    /// <summary>
    /// Lightweight TaskHelper that ensures a single running task per key/context.
    /// If a new call arrives with a different context, the previous run is cancelled (cooperatively) and replaced.
    /// </summary>
    public sealed class TaskHelper
    {
        private sealed class State
        {
            public object Context = default!;
            public CancellationTokenSource InternalCts = default!;
            public Task Task = Task.CompletedTask;
        }

        private readonly ConcurrentDictionary<string, State> _states = new();

        public Task RunSingleAsync<TContext>(string key, TContext context, Func<CancellationToken, Task> factory, CancellationToken externalToken)
        {
            var state = _states.AddOrUpdate(
                key,
                _ => CreateNewState(),
                (_, existing) =>
                {
                    if (Equals(existing.Context, context))
                        return existing;

                    existing.InternalCts.Cancel();
                    return CreateNewState();
                });

            return state.Task;

            State CreateNewState()
            {
                var internalCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);

                var newState = new State
                {
                    Context = context!,
                    InternalCts = internalCts
                };

                newState.Task = ExecuteAsync(newState, linkedCts);

                return newState;
            }

            async Task ExecuteAsync(State localState, CancellationTokenSource linkedCts)
            {
                try
                {
                    await factory(linkedCts.Token);
                }
                finally
                {
                    linkedCts.Dispose();
                    localState.InternalCts.Dispose();

                    if (_states.TryGetValue(key, out var current) && ReferenceEquals(current, localState))
                    {
                        _states.TryRemove(key, out _);
                    }
                }
            }
        }
    }
}