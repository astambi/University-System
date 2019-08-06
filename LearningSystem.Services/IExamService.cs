namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Services.Models.Users;

    public interface IExamService
    {
        Task<IEnumerable<ExamSubmissionServiceModel>> AllByStudentCourseAsync(int courseId, string userId);

        Task<bool> CreateAsync(int courseId, string userId, byte[] examFileBytes);

        Task<ExamDownloadServiceModel> DownloadAsync(int id);

        Task<bool> ExistsForStudentAsync(int id, string userId);
    }
}
