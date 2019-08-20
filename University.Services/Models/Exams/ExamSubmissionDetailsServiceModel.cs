namespace University.Services.Models.Exams
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ExamSubmissionDetailsServiceModel : ExamSubmissionServiceModel, IMapFrom<ExamSubmission>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
