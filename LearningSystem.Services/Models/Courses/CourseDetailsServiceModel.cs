namespace LearningSystem.Services.Models.Courses
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseDetailsServiceModel : CourseServiceModel, IMapFrom<Course>
    {
        public string Description { get; set; }

        public int StudentsCount { get; set; }

        public bool IsExamSubmissionDate
            => this.EndDate.ToLocalTime().Date == DateTime.Now.Date;
    }
}
