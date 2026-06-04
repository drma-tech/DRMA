using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Core;

public static class AppStateStatic
{
    public static Size Size { get; set; } = Size.Small;
    public static Breakpoint Breakpoint { get; set; } = Breakpoint.Xs;
    public static ActionDispatcher<Breakpoint> BreakpointChanged { get; } = new();

    public static string? Version { get; set; }
    public static string? BrowserName { get; set; }
    public static string? BrowserVersion { get; set; }
    public static string? OperatingSystem { get; set; }

    private static string? LastSnackbarMessage { get; set; }
    private static DateTime LastSnackbarAt { get; set; } = DateTime.MinValue;
    private static readonly TimeSpan SnackbarDelay = TimeSpan.FromSeconds(5);

    public static bool IsLocalhost(this NavigationManager navigation)
    {
        return navigation.BaseUri.Contains("localhost") || navigation.BaseUri.Contains("develop");
    }

    public static bool IsPrerendering(this NavigationManager navigation)
    {
        return navigation.BaseUri.Contains("127.0.0.1");
    }

    public static bool CanShowSnackbar(this string message)
    {
        var now = DateTime.UtcNow;

        if (LastSnackbarMessage == message &&
            now - LastSnackbarAt < SnackbarDelay)
        {
            return false;
        }

        LastSnackbarMessage = message;
        LastSnackbarAt = now;

        return true;
    }

    public static async Task<string> GetAppVersion(IJSRuntime js, CancellationToken cancellationToken)
    {
        try
        {
            var vs = await js.Utils().GetAppVersion(cancellationToken);

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

    public static async Task<Platform> GetPlatform(IJSRuntime js, CancellationToken cancellationToken)
    {
        await _platformSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_platform.HasValue)
            {
                return _platform.Value;
            }

            var cache = await js.Utils().GetStorage("platform", JavascriptContext.Default.NullablePlatform, cancellationToken);

            if (cache.HasValue)
            {
                _platform = cache.Value;
            }
            else
            {
                _platform = Platform.webapp;
                await js.Utils().SetStorage("platform", _platform, JavascriptContext.Default.NullablePlatform, cancellationToken);
            }

            return _platform.Value;
        }
        finally
        {
            _platformSemaphore.Release();
        }
    }

    #endregion Platform

    #region AppLanguage

    public static string[] SupportedLanguages => ["en"];

    private static AppLanguage? _appLanguage;
    private static readonly SemaphoreSlim _appLanguageSemaphore = new(1, 1);

    public static bool IsValidLanguage(this string? lang)
    {
        return SupportedLanguages.Contains(lang);
    }

    public static async Task<AppLanguage> GetAppLanguage(IJSRuntime js, CancellationToken cancellationToken)
    {
        await _appLanguageSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_appLanguage.HasValue)
            {
                return _appLanguage.Value;
            }

            var cache = await js.Utils().GetStorage("app-language", JavascriptContext.Default.NullableAppLanguage, cancellationToken);

            if (cache.HasValue)
            {
                _appLanguage = cache.Value;
            }
            else
            {
                var code = await js.Window().InvokeAsync<string>("eval", "navigator.language");
                code = code[..2].ToLowerInvariant();

                _appLanguage = ConvertAppLanguage(code, AppLanguage.en);
                await js.Utils().SetStorage("app-language", _appLanguage, JavascriptContext.Default.NullableAppLanguage, cancellationToken);
            }

            return _appLanguage.Value;
        }
        catch
        {
            return AppLanguage.en;
        }
        finally
        {
            _appLanguageSemaphore.Release();
        }
    }

    public static AppLanguage ConvertAppLanguage(string? code, AppLanguage fallback)
    {
        if (code.Empty()) return AppLanguage.en;

        if (System.Enum.TryParse<AppLanguage>(code, true, out var language) && System.Enum.IsDefined(language))
        {
            return language;
        }
        else
            return fallback;
    }

    public static string GetCulture(this NavigationManager navigation)
    {
        var segments = new Uri(navigation.Uri).AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var first = segments.FirstOrDefault()?.ToLowerInvariant();

        return first.IsValidLanguage() ? first! : "en";
    }

    #endregion AppLanguage

    #region DarkMode

    public static Action<bool>? DarkModeChanged { get; set; }

    private static bool? _darkMode;
    private static readonly SemaphoreSlim _darkModeSemaphore = new(1, 1);

    public static async Task<bool?> GetDarkMode(IJSRuntime js, CancellationToken cancellationToken)
    {
        await _darkModeSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_darkMode.HasValue)
            {
                return _darkMode.Value;
            }

            _darkMode = await js.Utils().GetStorage("dark-mode", JavascriptContext.Default.NullableBoolean, cancellationToken);

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

    public static TaskDispatcher ProcessingStarted { get; } = new();
    public static TaskDispatcher ProcessingFinished { get; } = new();
}