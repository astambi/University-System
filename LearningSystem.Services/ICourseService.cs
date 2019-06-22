namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;

    public interface ICourseService
    {
        Task<IEnumerable<CourseListingServiceModel>> AllWithTrainers(
            bool activeOnly = true,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        bool Exists(int id);

        Task<int> TotalAsync(bool activeOnly = true);

        Task<CourseDetailsServiceModel> GetById(int id);
    }
}
