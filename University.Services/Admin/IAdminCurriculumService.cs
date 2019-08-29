namespace University.Services.Admin
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Admin.Models.Curriculums;
    using University.Services.Admin.Models.Users;

    public interface IAdminCurriculumService
    {
        Task<bool> AddCourseAsync(int curriculumId, int courseId);

        Task<IEnumerable<AdminCurriculumServiceModel>> AllAsync();

        Task<int> CreateAsync(string name, string description);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsCurriculumCourseAsync(int curriculumId, int courseId);

        Task<AdminCurriculumBasicServiceModel> GetByIdAsync(int id);

        Task<IEnumerable<AdminUserListingServiceModel>> GetEligibleCandidatesWithoutDiplomasAsync(int id);

        Task<IEnumerable<AdminDiplomaGraduateServiceModel>> GetDiplomaGraduatesAsync(int id);

        Task<bool> RemoveAsync(int id);

        Task<bool> RemoveCourseAsync(int curriculumId, int courseId);

        Task<bool> UpdateAsync(int id, string name, string description);
    }
}
