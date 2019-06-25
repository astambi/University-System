namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;

    public interface ICourseService
    {
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

        bool Exists(int id);

        Task<CourseDetailsServiceModel> GetByIdAsync(int id);

        Task<bool> UserIsEnrolledInCourseAsync(int courseId, string userId);

        Task<int> TotalActiveAsync(string search = null);

        Task<int> TotalArchivedAsync(string search = null);
    }
}
