namespace University.Services.Admin.Models.Curriculums
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;
    using University.Services.Admin.Models.Users;

    public class AdminDiplomaGraduateServiceModel : IMapFrom<Diploma>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; }

        public int CurriculumId { get; set; }

        public AdminUserListingServiceModel Student { get; set; }
    }
}
