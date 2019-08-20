namespace University.Web.Infrastructure.Helpers
{
    using System;
    using University.Data;

    public class FileHelpers
    {
        public static string ExamFileName(string course, string student, DateTime submissionDate)
            => $"{course} - {student} - {submissionDate.ToLocalTime()}.{DataConstants.FileType}";
    }
}
