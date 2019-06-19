namespace LearningSystem.Services.Admin
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Services.Admin.Models;

    public interface IAdminCourseService
    {
        Task Create(
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            string trainerId);

        CourseEditServiceModel GetById(int id);

        Task UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            string trainerId);
    }
}
