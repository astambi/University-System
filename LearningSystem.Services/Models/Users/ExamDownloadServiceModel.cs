namespace LearningSystem.Services.Models.Users
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ExamDownloadServiceModel : IMapFrom<ExamSubmission>
    {
        public DateTime SubmissionDate { get; set; }

        public byte[] FileSubmission { get; set; }

        public string StudentUserName { get; set; }

        public string CourseName { get; set; }
    }
}
