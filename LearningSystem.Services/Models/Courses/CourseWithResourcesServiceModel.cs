namespace LearningSystem.Services.Models.Courses
{
    using System.Collections.Generic;

    public class CourseWithResourcesServiceModel
    {
        public CourseServiceModel Course { get; set; }

        public IEnumerable<CourseResourceServiceModel> Resources { get; set; }
    }
}
