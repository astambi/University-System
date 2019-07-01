namespace LearningSystem.Services.Models.Users
{
    using System;
    using LearningSystem.Data.Models;

    public class CertificateServiceModel
    {
        public string Id { get; set; }

        public string Course { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Student { get; set; }

        public Grade Grade { get; set; }

        public string Trainer { get; set; }

        public DateTime IssueDate { get; set; }

        public string DownloadUrl { get; set; }
    }
}
