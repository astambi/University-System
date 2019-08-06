namespace LearningSystem.Services.Models.Exams
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ExamSubmissionServiceModel : IMapFrom<ExamSubmission>
    {
        public int Id { get; set; }

        public DateTime SubmissionDate { get; set; }
    }
}
