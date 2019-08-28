namespace University.Services.Models.Users
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserDiplomaListingServiceModel : IMapFrom<Diploma>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; }

        public int CurriculumId { get; set; }

        public string CurriculumName { get; set; }
    }
}
