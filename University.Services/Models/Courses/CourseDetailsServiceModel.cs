namespace University.Services.Models.Courses
{
    using University.Common.Infrastructure.Extensions;
    using University.Common.Mapping;
    using University.Data.Models;

    public class CourseDetailsServiceModel : CourseServiceModel, IMapFrom<Course>
    {
        public string Description { get; set; }

        public string TrainerUsername { get; set; }

        public int StudentsCount { get; set; }

        public bool IsExamSubmissionDate
            => this.EndDate.IsToday();
    }
}
