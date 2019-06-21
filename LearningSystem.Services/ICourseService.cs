namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models;

    public interface ICourseService
    {
        Task<IEnumerable<CourseListingServiceModel>> AllWithTrainers(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        bool Exists(int id);

        Task<int> TotalAsync();
    }
}
