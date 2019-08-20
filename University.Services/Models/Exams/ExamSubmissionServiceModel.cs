namespace University.Services.Models.Exams
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class ExamSubmissionServiceModel : IMapFrom<ExamSubmission>
    {
        public int Id { get; set; }

        public DateTime SubmissionDate { get; set; }
    }
}
