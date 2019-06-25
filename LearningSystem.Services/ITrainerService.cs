namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public interface ITrainerService
    {
        Task AssessStudentCoursePerformance(string trainerId, int courseId, string studentId, Grade grade);

        Task<CourseServiceModel> CourseAsync(string id, int courseId);

        Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string id,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<bool> CourseHasEnded(int courseId);

        Task<bool> IsTrainerForCourseAsync(string userId, int courseId);

        Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId);

        Task<int> TotalCoursesAsync(string id, string search = null);
    }
}
