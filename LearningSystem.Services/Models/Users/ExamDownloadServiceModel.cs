namespace LearningSystem.Services.Models.Users
{
    using System;

    public class ExamDownloadServiceModel
    {
        public DateTime SubmissionDate { get; set; }

        public byte[] FileSubmission { get; set; }

        public string Student { get; set; }

        public string Course { get; set; }
    }
}
