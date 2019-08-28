namespace University.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Models.Exams;

    public interface IExamService
    {
        Task<IEnumerable<ExamSubmissionServiceModel>> AllByStudentCourseAsync(int courseId, string userId);

        Task<bool> CreateAsync(int courseId, string userId, byte[] examFileBytes);

        Task<ExamDownloadServiceModel> DownloadForStudentAsync(int id, string userId);

        Task<ExamDownloadServiceModel> DownloadForTrainerAsync(string trainerId, int courseId, string studentId);

        Task<bool> ExistsForStudentAsync(int id, string userId);

        Task<bool> EvaluateAsync(string trainerId, int courseId, string studentId, decimal gradeBg);
    }
}
