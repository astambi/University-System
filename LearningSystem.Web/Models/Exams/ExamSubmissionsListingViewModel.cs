namespace LearningSystem.Web.Models.Exams
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Exams;

    public class ExamSubmissionsListingViewModel
    {
        public IEnumerable<ExamSubmissionServiceModel> ExamSubmissions { get; set; }

        public CourseServiceModel Course { get; set; }
    }
}
