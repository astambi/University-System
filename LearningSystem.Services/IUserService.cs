namespace LearningSystem.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Certificates;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Services.Models.Resources;
    using LearningSystem.Services.Models.Users;

    public interface IUserService
    {
        Task<bool> CanBeDeletedAsync(string id);

        Task<UserEditServiceModel> GetProfileToEditAsync(string id);

        Task<UserProfileServiceModel> GetProfileAsync(string id);

        Task<IEnumerable<CourseProfileServiceModel>> GetCoursesAsync(string id);

        Task<IEnumerable<CertificatesByCourseServiceModel>> GetCertificatesAsync(string id);

        Task<IEnumerable<ExamsByCourseServiceModel>> GetExamsAsync(string id);

        Task<IEnumerable<ResourcesByCourseServiceModel>> GetResourcesAsync(string id);

        Task<bool> UpdateProfileAsync(string id, string name, DateTime birthdate);
    }
}
