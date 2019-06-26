namespace LearningSystem.Common.Infrastructure.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static DateTime ToStartDateUtc(this DateTime localDate)
            => new DateTime(localDate.Year, localDate.Month, localDate.Day, 00, 00, 00)
            .ToUniversalTime();

        public static DateTime ToEndDateUtc(this DateTime localDate)
            => new DateTime(localDate.Year, localDate.Month, localDate.Day, 23, 59, 59)
            .ToUniversalTime();
    }
}
