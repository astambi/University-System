namespace LearningSystem.Services.Models.Exams
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ExamSubmissionDetailsServiceModel : ExamSubmissionServiceModel, IMapFrom<ExamSubmission>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
