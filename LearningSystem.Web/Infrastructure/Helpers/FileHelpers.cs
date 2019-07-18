namespace LearningSystem.Web.Infrastructure.Helpers
{
    using System;
    using LearningSystem.Data;

    public class FileHelpers
    {
        public static string ExamFileName(string course, string student, DateTime submissionDate)
            => $"{course} - {student} - {submissionDate.ToLocalTime()}.{DataConstants.FileType}";
    }
}
