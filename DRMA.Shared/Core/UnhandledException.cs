namespace DRMA.Shared.Core;

public class UnhandledException : Exception
{
    public UnhandledException()
    {
    }

    public UnhandledException(string? message) : base(message)
    {
    }

    public UnhandledException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}