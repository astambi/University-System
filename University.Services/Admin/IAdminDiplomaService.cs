namespace University.Services.Admin
{
    using System.Threading.Tasks;

    public interface IAdminDiplomaService
    {
        Task<bool> CreateAsync(int curriculumId, string studentId);

        Task<bool> ExistsAsync(string id);

        Task<bool> ExistsForCurriculumStudentAsync(int curriculumId, string studentId);

        Task<bool> HasPassedAllCurriculumCoursesAsync(int curriculumId, string studentId);

        Task<bool> RemoveAsync(string id);
    }
}
