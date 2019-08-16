namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.ShoppingCart;

    public interface ICourseService
    {
        Task<IEnumerable<CourseServiceModel>> AllActiveAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<IEnumerable<CourseServiceModel>> AllArchivedAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task<bool> CancellUserEnrollmentInCourseAsync(int courseId, string userId);

        Task<bool> CancelUserEnrollmentInOrderCoursesAsync(int orderId, string userId);

        Task<bool> CanEnrollAsync(int id);

        Task<bool> EnrollUserInCourseAsync(int courseId, string userId);

        Task<bool> EnrollUserInOrderCoursesAsync(int orderId, string userId);

        bool Exists(int id);

        Task<CourseServiceModel> GetByIdBasicAsync(int id);

        Task<CourseDetailsServiceModel> GetByIdAsync(int id);

        Task<IEnumerable<CartItemDetailsServiceModel>> GetCartItemsDetailsForUserAsync(IEnumerable<CartItem> cartItems, string userId = null);

        Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId);

        Task<int> TotalActiveAsync(string search = null);

        Task<int> TotalArchivedAsync(string search = null);
    }
}
