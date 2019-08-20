namespace University.Services.Models.Users
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserCertificateServiceModel : IMapFrom<Certificate>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public Grade Grade { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
