using Blazorise;

namespace DRMA.WEB.Core
{
    public static class NotificationCore
    {
        public static async Task ProcessResponse(this HttpResponseMessage response, INotificationService? toast = null, string? msgSuccess = null, string? msgInfo = null)
        {
            var msg = await response.Content.ReadAsStringAsync();

            if ((short)response.StatusCode is >= 100 and <= 199) //Provisional response
            {
                //do nothing
            }
            else if ((short)response.StatusCode is >= 200 and <= 299) //Successful
            {
                if (!string.IsNullOrEmpty(msgSuccess)) toast?.Success(msgSuccess);
                if (!string.IsNullOrEmpty(msgInfo)) toast?.Info(msgInfo);
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

        public static void ProcessException(this Exception ex, INotificationService toast, ILogger logger)
        {
            if (ex is NotificationException exc)
            {
                logger.LogWarning(exc, null);
                toast.Warning(exc.Message);
            }
            else
            {
                logger.LogError(ex, null);
                toast.Error(ex.Message);
            }
        }
    }
}