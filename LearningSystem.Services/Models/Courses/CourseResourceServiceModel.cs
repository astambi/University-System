namespace LearningSystem.Services.Models.Courses
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseResourceServiceModel : IMapFrom<Resource>
    {
        public int Id { get; set; }

        public string FileName { get; set; }
    }
}
