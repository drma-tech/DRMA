namespace DRMA.WEB.Core;

public static class DataHelper
{
    public static string GetResume(this string? text, int count)
    {
        if (string.IsNullOrEmpty(text)) return "";

        return text.Length > count ? string.Concat(text.AsSpan(0, count), "...") : text;
    }

    public static string GetElapsedTime(this DateTime date)
    {
        return GetElapsedTime((DateTimeOffset)date);
    }

    public static string GetElapsedTime(this DateTimeOffset date)
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(DateTime.UtcNow.ToLocalTime().Ticks - date.ToLocalTime().Ticks);
        var delta = Math.Abs(ts.TotalSeconds);

        switch (delta)
        {
            case < 1 * MINUTE:
                return ts.Seconds <= 1 ? "Now" : ts.Seconds + " seconds ago";

            case < 2 * MINUTE:
                return "a minute ago";

            case < 45 * MINUTE:
                return ts.Minutes + " minutes ago";

            case < 90 * MINUTE:
                return "One hour ago";

            case < 24 * HOUR:
                return ts.Hours + " hours ago";

            case < 48 * HOUR:
                return "yesterday";

            case < 30 * DAY:
                return ts.Days + " days ago";

            case < 12 * MONTH:
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "A month ago" : months + " months ago";
            }
            default:
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "One year ago" : years + " years ago";
            }
        }
    }
}
