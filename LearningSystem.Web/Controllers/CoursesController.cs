namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using AutoMapper;
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

            var courses = await this.courseService.AllActiveWithTrainers(pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination
            };

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

            var courses = await this.courseService.AllArchivedWithTrainers(pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination
            };

            return this.View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await this.courseService.GetById(id);
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

            course.IsUserEnrolled = await this.courseService.UserIsEnrolledInCourse(id, userId);

            return this.View(course);
        }

        [Authorize]
        public async Task<IActionResult> Enroll(int id)
            => await this.UpdateEnrollment(id, true);

        [Authorize]
        public async Task<IActionResult> Cancel(int id)
            => await this.UpdateEnrollment(id, false);

        private async Task<IActionResult> UpdateEnrollment(int id, bool enrollAction)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
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

            var canEnroll = await this.courseService.CanEnroll(id);
            if (!canEnroll)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentClosedMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isEnrolledInCourse = await this.courseService.UserIsEnrolledInCourse(id, userId);

            switch (enrollAction)
            {
                // Enroll
                case true:
                    if (isEnrolledInCourse)
                    {
                        this.TempData.AddInfoMessage(WebConstants.UserAlreadyEnrolledInCourseMsg);
                        return this.RedirectToAction(nameof(Index));
                    }

                    await this.courseService.EnrollUserInCourse(id, userId);
                    this.TempData.AddSuccessMessage(WebConstants.UserEnrolledInCourseMsg);
                    break;
                // Cancel enrollment
                case false:
                    if (!isEnrolledInCourse)
                    {
                        this.TempData.AddInfoMessage(WebConstants.UserNotEnrolledInCourseMsg);
                        return this.RedirectToAction(nameof(Index));
                    }

                    await this.courseService.CancellUserEnrollmentInCourse(id, userId);
                    this.TempData.AddSuccessMessage(WebConstants.UserCancelledEnrollmentInCourseMsg);
                    break;
            }

            return this.RedirectToAction(nameof(Details), routeValues: new { id });
        }
    }
}