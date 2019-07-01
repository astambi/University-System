namespace LearningSystem.Common.Infrastructure.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static DateTime ToStartDateUtc(this DateTime localDate)
            => new DateTime(localDate.Year, localDate.Month, localDate.Day)
            .ToUniversalTime(); // 00:00:00

        public static DateTime ToEndDateUtc(this DateTime localDate)
            => ToStartDateUtc(localDate)
            .AddDays(1)
            .AddSeconds(-1); // 23:59:59
    }
}
