namespace LearningSystem.Web.Models.Trainers
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public class StudentCourseGradeViewModel
    {
        public IEnumerable<StudentInCourseServiceModel> Students { get; set; }

        public CourseServiceModel Course { get; set; }
    }
}
