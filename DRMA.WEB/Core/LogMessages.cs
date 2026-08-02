namespace DRMA.WEB.Core
{
    public static partial class LogMessages
    {
        [LoggerMessage(Level = LogLevel.Warning, Message = "{Message}")]
        public static partial void Warning(this ILogger logger, string message);

        [LoggerMessage(Level = LogLevel.Error, Message = "{Message}")]
        public static partial void Error(this ILogger logger, Exception exception, string message);
    }
}