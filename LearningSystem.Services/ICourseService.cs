namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;

    public interface ICourseService
    {
        Task<IEnumerable<CourseListingServiceModel>> AllActiveWithTrainers(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<IEnumerable<CourseListingServiceModel>> AllArchivedWithTrainers(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<bool> CanEnroll(int id);

        Task EnrollUserInCourse(int courseId, string userId);

        Task CancellUserEnrollmentInCourse(int courseId, string userId);

        bool Exists(int id);

        Task<CourseDetailsServiceModel> GetById(int id);

        Task<bool> UserIsEnrolledInCourse(int courseId, string userId);

        Task<int> TotalActiveAsync();

        Task<int> TotalArchivedAsync();
    }
}
