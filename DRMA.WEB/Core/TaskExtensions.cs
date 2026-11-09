using System.Runtime.CompilerServices;

namespace DRMA.WEB.Core;

public static class TaskExtensions
{
    public static void FireAndForget(this Task task, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompletedSuccessfully || task.IsCanceled) return;

        _ = ForgetAwaited(task, caller, file, line);
    }

    private static async Task ForgetAwaited(Task task, string caller, string file, int line)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex) when (ex.Message.Contains("AbortError", StringComparison.OrdinalIgnoreCase))
        {
            // ignore
        }
        catch (Exception ex)
        {
            ex.Data["fire_and_forget.task_status"] = task.Status.ToString();
            ex.Data["fire_and_forget.caller"] = caller;
            ex.Data["fire_and_forget.file"] = Path.GetFileName(file);
            ex.Data["fire_and_forget.line"] = line;

            SentrySdk.CaptureException(ex);
        }
    }
}