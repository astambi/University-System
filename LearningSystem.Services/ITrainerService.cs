namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public interface ITrainerService
    {
        Task<CourseServiceModel> CourseAsync(string id, int courseId);

        Task<IEnumerable<CourseServiceModel>> CoursesAsync(string id);

        Task<bool> CourseHasEnded(int courseId);

        Task<bool> IsTrainerForCourseAsync(string userId, int courseId);

        Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId);

        Task AssessStudentCoursePerformance(string trainerId, int courseId, string studentId, Grade grade);
    }
}
