namespace University.Common.Infrastructure.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        private const string DateMonthYearFormat = "MMMM yyyy";

        public static int DaysTo(this DateTime startDateTime, DateTime endDateTime)
            => endDateTime
            .AddDays(1)
            .Subtract(startDateTime)
            .Days;

        public static TimeSpan RemainingTimeTillStart(this DateTime dateTimeUtc)
            => dateTimeUtc.Subtract(DateTime.UtcNow);

        public static bool HasEnded(this DateTime dateTimeUtc)
           => dateTimeUtc < DateTime.UtcNow;

        public static bool IsToday(this DateTime dateTimeUtc)
            => dateTimeUtc.ToLocalTime().Date == DateTime.Now.Date;

        public static bool IsUpcoming(this DateTime dateTimeUtc)
            => DateTime.UtcNow < dateTimeUtc;

        public static string ToDateString(this DateTime date)
            => date.ToString(DateMonthYearFormat);

        public static DateTime ToStartDateUtc(this DateTime localDate)
            => new DateTime(localDate.Year, localDate.Month, localDate.Day)
            .ToUniversalTime(); // 00:00:00

        public static DateTime ToEndDateUtc(this DateTime localDate)
            => ToStartDateUtc(localDate)
            .AddDays(1)
            .AddSeconds(-1); // 23:59:59
    }
}
