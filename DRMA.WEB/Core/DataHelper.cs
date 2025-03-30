namespace DRMA.WEB.Core
{
    public static class DataHelper
    {
        public static string GetResume(this string? text, int count)
        {
            if (string.IsNullOrEmpty(text)) return "";

            if (text.Length > count)
            {
                return string.Concat(text.AsSpan(0, count), "...");
            }
            else
            {
                return text;
            }
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
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds <= 1 ? "Now" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "One hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "A month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "One year ago" : years + " years ago";
            }
        }
    }
}
