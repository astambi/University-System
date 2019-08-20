namespace University.Services.Models.Exams
{
    using System.Collections.Generic;

    public class ExamsByCourseServiceModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public IEnumerable<ExamSubmissionServiceModel> Exams { get; set; }
    }
}
