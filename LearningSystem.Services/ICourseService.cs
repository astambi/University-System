namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;

    public interface ICourseService
    {
        Task AddExamSubmissionAsync(int id, string userId, byte[] examFileBytes);

        Task<IEnumerable<CourseServiceModel>> AllActiveWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<IEnumerable<CourseServiceModel>> AllArchivedWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<bool> CanEnrollAsync(int id);

        Task EnrollUserInCourseAsync(int courseId, string userId);

        Task CancellUserEnrollmentInCourseAsync(int courseId, string userId);

        Task<IEnumerable<CourseStudentExamSubmissionServiceModel>> ExamSubmisionsAsync(int courseId, string userId);

        bool Exists(int id);

        Task<CourseDetailsServiceModel> GetByIdAsync(int id);

        IQueryable<Course> GetQuerableBySearch(string search);

        IQueryable<Course> GetQuerableByStatus(IQueryable<Course> coursesAsQuerable, bool? isActive = null); // all by default

        bool IsGradeEligibleForCertificate(Grade? grade);

        Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId);

        Task<int> TotalActiveAsync(string search = null);

        Task<int> TotalArchivedAsync(string search = null);
    }
}
