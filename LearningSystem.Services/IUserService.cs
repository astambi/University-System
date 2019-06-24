namespace LearningSystem.Services
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Users;

    public interface IUserService
    {
        Task<UserProfileServiceModel> GetUserProfileAsync(string id);

        Task<UserEditServiceModel> GetProfileToEditAsync(string id);

        Task UpdateUserProfileAsync(string id, string name, DateTime birthdate);
    }
}
