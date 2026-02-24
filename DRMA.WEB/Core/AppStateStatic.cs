using DRMA.Shared.Core.Helper;
using DRMA.WEB.Modules.Subscription.Core;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace DRMA.WEB.Core;

public static class AppStateStatic
{
    public static Breakpoint Breakpoint { get; set; } = Breakpoint.Xs;
    public static Action<Breakpoint>? BreakpointChanged { get; set; }
    public static Size Size { get; set; } = Size.Small;

    public static BrowserWindowSize? BrowserWindowSize { get; set; }
    public static Action<BrowserWindowSize>? BrowserWindowSizeChanged { get; set; }

    public static string? Version { get; set; }
    public static string? BrowserName { get; set; }
    public static string? BrowserVersion { get; set; }

    public static string? OperatingSystem { get; set; }
    public static string? UserAgent { get; set; }

    public static async Task<string> GetAppVersion(IJSRuntime js)
    {
        try
        {
            var vs = await js.Utils().GetAppVersion();

            return vs?.ReplaceLineEndings("").Trim() ?? "version-error";
        }
        catch (Exception)
        {
            return "version-error";
        }
    }

    #region Platform

    private static Platform? _platform;
    private static readonly SemaphoreSlim _platformSemaphore = new(1, 1);

    public static Platform? GetSavedPlatform()
    {
        return _platform;
    }

    public static async Task<Platform> GetPlatform(IJSRuntime js)
    {
        await _platformSemaphore.WaitAsync();
        try
        {
            if (_platform.HasValue)
            {
                return _platform.Value;
            }

            var cache = await js.Utils().GetStorage<Platform?>("platform");

            if (cache.HasValue)
            {
                _platform = cache.Value;
            }
            else
            {
                _platform = Platform.webapp;
                await js.Utils().SetStorage("platform", _platform);
            }

            return _platform.Value;
        }
        finally
        {
            _platformSemaphore.Release();
        }
    }

    #endregion Platform

    #region DarkMode

    public static Action<bool>? DarkModeChanged { get; set; }

    private static bool? _darkMode;
    private static readonly SemaphoreSlim _darkModeSemaphore = new(1, 1);

    public static async Task<bool?> GetDarkMode(IJSRuntime js)
    {
        await _darkModeSemaphore.WaitAsync();
        try
        {
            if (_darkMode.HasValue)
            {
                return _darkMode.Value;
            }

            _darkMode = await js.Utils().GetStorage<bool?>("dark-mode");

            return _darkMode;
        }
        catch
        {
            return null;
        }
        finally
        {
            _darkModeSemaphore.Release();
        }
    }

    public static void ChangeDarkMode(bool darkMode)
    {
        _darkMode = darkMode;
        DarkModeChanged?.Invoke(darkMode);
    }

    #endregion DarkMode

    public static Action<string>? ShowError { get; set; }
    public static Action? ProcessingStarted { get; set; }
    public static Action? ProcessingFinished { get; set; }
}