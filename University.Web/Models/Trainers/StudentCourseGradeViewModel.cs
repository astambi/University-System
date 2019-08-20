namespace University.Web.Models.Trainers
{
    using System.Collections.Generic;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;

    public class StudentCourseGradeViewModel
    {
        public IEnumerable<StudentInCourseServiceModel> Students { get; set; }

        public CourseServiceModel Course { get; set; }
    }
}
