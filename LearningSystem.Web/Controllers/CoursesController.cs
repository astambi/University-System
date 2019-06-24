namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class CoursesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICourseService courseService;

        public CoursesController(
            UserManager<User> userManager,
            ICourseService courseService)
        {
            this.userManager = userManager;
            this.courseService = courseService;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            var pagination = new PaginationModel
            {
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalActiveAsync()
            };

            var courses = await this.courseService.AllActiveWithTrainersAsync(pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel { Courses = courses, Pagination = pagination };

            return this.View(model);
        }

        public async Task<IActionResult> Archive(int currentPage = 1)
        {
            var pagination = new PaginationModel
            {
                Action = nameof(Archive),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalArchivedAsync()
            };

            var courses = await this.courseService.AllArchivedWithTrainersAsync(pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel { Courses = courses, Pagination = pagination };

            return this.View(model);
        }

        [Authorize]
        public async Task<IActionResult> Cancel(int id)
            => await this.UpdateEnrollment(id, false);

        public async Task<IActionResult> Details(int id)
        {
            var course = await this.courseService.GetByIdAsync(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            course.IsUserEnrolled = await this.courseService.UserIsEnrolledInCourseAsync(id, userId);

            return this.View(course);
        }

        [Authorize]
        public async Task<IActionResult> Enroll(int id)
            => await this.UpdateEnrollment(id, true);

        private async Task<IActionResult> TryCancel(int id, string userId, bool isEnrolled)
        {
            if (!isEnrolled)
            {
                this.TempData.AddInfoMessage(WebConstants.UserNotEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            await this.courseService.CancellUserEnrollmentInCourseAsync(id, userId);
            this.TempData.AddSuccessMessage(WebConstants.UserCancelledEnrollmentInCourseMsg);

            return this.RedirectToAction(nameof(Details), routeValues: new { id });
        }

        private async Task<IActionResult> TryEnroll(int id, string userId, bool isEnrolled)
        {
            if (isEnrolled)
            {
                this.TempData.AddInfoMessage(WebConstants.UserAlreadyEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            await this.courseService.EnrollUserInCourseAsync(id, userId);
            this.TempData.AddSuccessMessage(WebConstants.UserEnrolledInCourseMsg);

            return this.RedirectToAction(nameof(Details), routeValues: new { id });
        }

        private async Task<IActionResult> UpdateEnrollment(int id, bool enrollAction) // Enroll / Cancel
        {
            if (!this.courseService.Exists(id))
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            if (!await this.courseService.CanEnrollAsync(id))
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentClosedMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isEnrolled = await this.courseService.UserIsEnrolledInCourseAsync(id, userId);

            return enrollAction
                ? await this.TryEnroll(id, userId, isEnrolled)
                : await this.TryCancel(id, userId, isEnrolled);
        }
    }
}