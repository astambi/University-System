namespace LearningSystem.Web.Infrastructure.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static string ToDate(this DateTime dateTime)
            => dateTime.ToLocalTime().ToShortDateString();
    }
}
