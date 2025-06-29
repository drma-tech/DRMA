using MudBlazor;

namespace DRMA.WEB.Core;

public static class NotificationCore
{
    public static async Task ProcessResponse(this HttpResponseMessage response, ISnackbar? snackbar = null,
        string? msgSuccess = null, string? msgInfo = null)
    {
        var msg = await response.Content.ReadAsStringAsync();

        if ((short)response.StatusCode is >= 100 and <= 199) //Provisional response
        {
            //do nothing
        }
        else if ((short)response.StatusCode is >= 200 and <= 299) //Successful
        {
            if (!string.IsNullOrEmpty(msgSuccess)) snackbar?.Add(msgSuccess, Severity.Success);
            if (!string.IsNullOrEmpty(msgInfo)) snackbar?.Add(msgInfo, Severity.Info);
        }
        else if ((short)response.StatusCode is >= 300 and <= 399) //Redirected
        {
            throw new NotificationException(msg);
        }
        else if ((short)response.StatusCode is >= 400 and <= 499) //Request error
        {
            throw new NotificationException(msg);
        }
        else //Server error
        {
            throw new InvalidOperationException(msg);
        }
    }

    public static void ProcessException(this Exception ex, ISnackbar snackbar, ILogger logger)
    {
        if (ex is NotificationException exc)
        {
            logger.LogWarning(exc, null);
            snackbar.Add(exc.Message, Severity.Warning);
        }
        else
        {
            logger.LogError(ex, null);
            snackbar.Add(ex.Message, Severity.Error);
        }
    }
}