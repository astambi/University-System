namespace University.Services.Models.Certificates
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class CertificateServiceModel : IMapFrom<Certificate>
    {
        public string Id { get; set; }

        public string CourseName { get; set; }

        public DateTime CourseStartDate { get; set; }

        public DateTime CourseEndDate { get; set; }

        public string StudentName { get; set; }

        public decimal GradeBg { get; set; }

        public string CourseTrainerName { get; set; }

        public DateTime IssueDate { get; set; }

        public string DownloadUrl { get; set; }
    }
}
