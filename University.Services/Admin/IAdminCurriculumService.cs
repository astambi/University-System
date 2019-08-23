namespace University.Services.Admin
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Admin.Models;

    public interface IAdminCurriculumService
    {
        Task<bool> AddCourseAsync(int curriculumId, int courseId);

        Task<IEnumerable<AdminCurriculumServiceModel>> AllAsync();

        Task<int> CreateAsync(string name, string description);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsCurriculumCourseAsync(int curriculumId, int courseId);

        Task<bool> RemoveCourseAsync(int curriculumId, int courseId);
    }
}
