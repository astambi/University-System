namespace University.Services.Models.Diplomas
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class DiplomaServiceModel : IMapFrom<Diploma>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; }

        public string StudentId { get; set; }

        public string StudentName { get; set; }

        public string CurriculumName { get; set; }

        public string DownloadUrl { get; set; }
    }
}
