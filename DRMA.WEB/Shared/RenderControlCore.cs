namespace DRMA.WEB.Shared;

public enum LoadingStatus
{
    Loading,
    Error,
    Warning,
    ShowContent
}

public class RenderControlCore<T>
{
    public Action? LoadingStarted { get; set; }
    public Action<T?>? LoadingFinished { get; set; }
    public Action? ProcessingStarted { get; set; }
    public Action<T?>? ProcessingFinished { get; set; }
    public Action<string?>? WarningAction { get; set; }
    public Action<string?>? ErrorAction { get; set; }

    public void ShowLoading()
    {
        LoadingStarted?.Invoke();
    }

    public void HideLoading(T? data)
    {
        LoadingFinished?.Invoke(data);
    }

    public void ShowProcessing()
    {
        ProcessingStarted?.Invoke();
    }

    public void HideProcessing(T? data)
    {
        ProcessingFinished?.Invoke(data);
    }

    public void ShowWarning(string? msg)
    {
        WarningAction?.Invoke(msg);
    }

    public void ShowError(string? msg)
    {
        ErrorAction?.Invoke(msg);
    }
}