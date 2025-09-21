using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace DRMA.WEB.Core;

public static class AppStateStatic
{
    public static Breakpoint Breakpoint { get; set; }
    public static Action<Breakpoint>? BreakpointChanged { get; set; }

    public static BrowserWindowSize? BrowserWindowSize { get; set; }
    public static Action<BrowserWindowSize>? BrowserWindowSizeChanged { get; set; }

    #region Platform

    private static Platform? _platform;
    private static readonly SemaphoreSlim _platformSemaphore = new(1, 1);

    public static async Task<Platform> GetPlatform(IJSRuntime? js = null)
    {
        await _platformSemaphore.WaitAsync();
        try
        {
            if (_platform.HasValue)
            {
                return _platform.Value;
            }

            var cache = js != null ? await js.InvokeAsync<string>("GetLocalStorage", "platform") : null;

            if (cache.Empty() && js != null) //shouldn't happen (because it's called in index.html)
            {
                await js.InvokeVoidAsync("LoadAppVariables");
                cache = await js.InvokeAsync<string>("GetLocalStorage", "platform");
            }

            if (cache.NotEmpty())
            {
                if (Enum.TryParse<Platform>(cache, true, out var platform) && Enum.IsDefined(platform))
                {
                    _platform = platform;
                }
                else
                {
                    _platform = Platform.webapp;
                    if (js != null) await js.InvokeVoidAsync("SetLocalStorage", "platform", _platform.ToString());
                }
            }
            else
            {
                _platform = Platform.webapp;
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

    public static async Task<bool?> GetDarkMode(IJSRuntime? js = null)
    {
        await _darkModeSemaphore.WaitAsync();
        try
        {
            if (_darkMode.HasValue)
            {
                return _darkMode.Value;
            }

            var cache = js != null ? await js.InvokeAsync<string>("GetLocalStorage", "dark-mode") : null;

            if (cache.NotEmpty())
            {
                if (bool.TryParse(cache, out var value))
                {
                    _darkMode = value;
                }
                else
                {
                    _darkMode = false;
                    if (js != null) await js.InvokeVoidAsync("SetLocalStorage", "dark-mode", _darkMode.ToString()?.ToLower());
                }
            }

            return _darkMode;
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
