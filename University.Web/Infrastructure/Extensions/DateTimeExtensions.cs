namespace University.Web.Infrastructure.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static string ToDate(this DateTime dateTime)
            => dateTime
            .ToLocalTime()
            .ToShortDateString();

        public static string ToDays(this int days)
        {
            if (days > 0)
            {
                return days == 1 ? $"{days} day" : $"{days} days";
            }

            return null;
        }

        public static string ToDaysOrHours(this TimeSpan timeSpan)
        {
            var days = timeSpan.Days;
            var hours = timeSpan.Hours;

            if (days > 0)
            {
                return days == 1 ? $"{days} day" : $"{days} days";
            }

            if (hours > 0)
            {
                return hours == 1 ? $"{hours} hour" : $"{hours} hours";

            }

            return null;
        }
    }
}
