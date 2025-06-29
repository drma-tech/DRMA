namespace DRMA.WEB.Core;

public static class AppStateStatic
{
    public static List<LogContainer> Logs { get; private set; } = [];

    [Custom(Name = "Dark Mode")]
    public static bool DarkMode { get; private set; }

    public static Action? DarkModeChanged { get; set; }

    public static Action<string>? ShowError { get; set; }

    public static void ChangeDarkMode(bool darkMode)
    {
        DarkMode = darkMode;
        DarkModeChanged?.Invoke();
    }
}