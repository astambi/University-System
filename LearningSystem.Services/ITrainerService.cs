﻿namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public interface ITrainerService
    {
        Task<CourseServiceModel> CourseByIdAsync(string trainerId, int courseId);

        Task<CourseWithResourcesServiceModel> CourseWithResourcesByIdAsync(string trainerId, int courseId);

        Task<bool> CourseHasEndedAsync(int courseId);

        Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string trainerId,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<UserServiceModel> GetProfileAsync(string trainerId);

        Task<bool> IsTrainerForCourseAsync(string userId, int courseId);

        Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId);

        Task<int> TotalCoursesAsync(string trainerId, string search = null);
    }
}
