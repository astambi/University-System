namespace LearningSystem.Services.Models.Courses
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseStudentExamSubmissionServiceModel : IMapFrom<ExamSubmission>
    {
        public int Id { get; set; }

        public DateTime SubmissionDate { get; set; }
    }
}
