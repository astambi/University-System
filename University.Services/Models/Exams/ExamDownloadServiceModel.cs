namespace University.Services.Models.Exams
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class ExamDownloadServiceModel : IMapFrom<ExamSubmission>
    {
        public DateTime SubmissionDate { get; set; }

        public byte[] FileSubmission { get; set; }

        public string StudentUserName { get; set; }

        public string CourseName { get; set; }
    }
}
