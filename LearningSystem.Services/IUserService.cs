namespace LearningSystem.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public interface IUserService
    {
        Task<bool> CanBeDeletedAsync(string id);

        Task<UserEditServiceModel> GetProfileToEditAsync(string id);

        Task<UserWithBirthdateServiceModel> GetUserProfileDataAsync(string id);

        Task<IEnumerable<CourseProfileServiceModel>> GetUserProfileCoursesAsync(string id);

        Task<bool> UpdateUserProfileAsync(string id, string name, DateTime birthdate);
    }
}
