namespace LearningSystem.Services.Admin
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Services.Admin.Models;

    public interface IAdminCourseService
    {
        Task<int> CreateAsync(
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            decimal price,
            string trainerId);

        Task<AdminCourseServiceModel> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            decimal price,
            string trainerId);

        Task<bool> RemoveAsync(int id);
    }
}
