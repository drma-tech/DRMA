using Blazorise;

namespace DRMA.WEB.Core
{
    public static class AppStateStatic
    {
        public static List<LogContainer> Logs { get; private set; } = [];

        public static Action<string>? ShowError { get; set; }
    }
}