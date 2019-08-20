namespace University.Web.Models.Exams
{
    using System.Collections.Generic;
    using University.Services.Models.Courses;
    using University.Services.Models.Exams;

    public class ExamSubmissionsListingViewModel
    {
        public IEnumerable<ExamSubmissionServiceModel> ExamSubmissions { get; set; }

        public CourseServiceModel Course { get; set; }
    }
}
