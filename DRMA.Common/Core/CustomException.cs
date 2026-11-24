namespace DRMA.Common.Core;

public class InvestigationException : Exception
{
    public InvestigationException()
    {
    }

    public InvestigationException(string? message) : base(message)
    {
    }

    public InvestigationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class NotificationException : Exception
{
    public NotificationException()
    {
    }

    public NotificationException(string? message) : base(message)
    {
    }

    public NotificationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}