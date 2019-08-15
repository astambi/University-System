namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.ShoppingCart;

    public interface ICourseService
    {
        Task<IEnumerable<CourseServiceModel>> AllActiveWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<IEnumerable<CourseServiceModel>> AllArchivedWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task CancellUserEnrollmentInCourseAsync(int courseId, string userId);

        Task<bool> CancelUserEnrollmentInOrderCoursesAsync(int orderId, string userId);

        Task<bool> CanEnrollAsync(int id);

        Task EnrollUserInCourseAsync(int courseId, string userId);

        Task<bool> EnrollUserInOrderCoursesAsync(int orderId, string userId);

        bool Exists(int id);

        Task<CourseServiceModel> GetBasicByIdAsync(int id);

        Task<CourseDetailsServiceModel> GetByIdAsync(int id);

        Task<IEnumerable<CartItemDetailsServiceModel>> GetCartItemsDetailsForUserAsync(IEnumerable<CartItem> cartItems, string userId = null);

        IQueryable<Course> GetBySearch(string search);

        IQueryable<Course> GetByStatus(IQueryable<Course> coursesAsQuerable, bool? isActive = null); // all by default

        Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId);

        Task<int> TotalActiveAsync(string search = null);

        Task<int> TotalArchivedAsync(string search = null);
    }
}
