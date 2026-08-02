namespace DRMA.WEB.Core.Component
{
    public abstract class ComponentParameterCore<T> : ComponentCore<T> where T : class
    {
        /// <summary>
        /// To load temporary data that may change and depends on parameters.
        /// </summary>
        /// <returns></returns>
        protected abstract Task LoadParameterDataAsync();

        private IReadOnlyList<string?> _lastParameterKey = [];

        protected abstract IReadOnlyList<string?> GetParameterKey();

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                await base.OnParametersSetAsync();

                var parameterKey = GetParameterKey();

                if (!AreParametersEqual(_lastParameterKey, parameterKey))
                {
                    _lastParameterKey = [.. parameterKey];
                    await LoadParameterDataAsync();
                }
            }
            catch (Exception ex)
            {
                await ProcessException(ex, ShowExceptions);
            }
        }

        private static bool AreParametersEqual(IReadOnlyList<string?> previous, IReadOnlyList<string?> current)
        {
            if (previous.Count != current.Count)
            {
                return false;
            }

            for (var i = 0; i < current.Count; i++)
            {
                if (!string.Equals(previous[i], current[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        protected static string GetDictionaryKey(IDictionary<string, string> dictionary)
        {
            return string.Join('|', dictionary.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Select(x => $"{x.Key}={x.Value}"));
        }

        protected static string GetCollectionKey(IEnumerable<string?> items)
        {
            return string.Join('|', items.Order(StringComparer.OrdinalIgnoreCase).Select(x => x));
        }
    }
}