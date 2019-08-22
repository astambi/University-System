namespace University.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Models.Certificates;
    using University.Services.Models.Courses;
    using University.Services.Models.Exams;
    using University.Services.Models.Resources;
    using University.Services.Models.Users;

    public interface IUserService
    {
        Task<bool> CanBeDeletedAsync(string id);

        Task<UserEditServiceModel> GetProfileToEditAsync(string id);

        Task<UserProfileServiceModel> GetProfileAsync(string id);

        Task<IEnumerable<CourseProfileMaxGradeServiceModel>> GetCoursesAsync(string id);

        Task<IEnumerable<CertificatesByCourseServiceModel>> GetCertificatesAsync(string id);

        Task<IEnumerable<ExamsByCourseServiceModel>> GetExamsAsync(string id);

        Task<IEnumerable<ResourcesByCourseServiceModel>> GetResourcesAsync(string id);

        Task<bool> UpdateProfileAsync(string id, string name, DateTime birthdate);
    }
}
