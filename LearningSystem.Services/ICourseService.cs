namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;

    public interface ICourseService
    {
        Task<IEnumerable<CourseServiceModel>> AllActiveWithTrainersAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<IEnumerable<CourseServiceModel>> AllArchivedWithTrainersAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<bool> CanEnrollAsync(int id);

        Task EnrollUserInCourseAsync(int courseId, string userId);

        Task CancellUserEnrollmentInCourseAsync(int courseId, string userId);

        bool Exists(int id);

        Task<CourseDetailsServiceModel> GetByIdAsync(int id);

        Task<bool> UserIsEnrolledInCourseAsync(int courseId, string userId);

        Task<int> TotalActiveAsync();

        Task<int> TotalArchivedAsync();
    }
}
