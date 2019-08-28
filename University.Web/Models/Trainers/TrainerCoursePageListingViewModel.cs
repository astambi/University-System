namespace University.Web.Models.Trainers
{
    using System.Collections.Generic;
    using University.Services.Models.Courses;
    using University.Web.Models.Courses;

    public class TrainerCoursePageListingViewModel : CoursePageListingViewModel
    {
        public IEnumerable<CourseServiceModel> CoursesToEvaluate { get; set; }
    }
}
