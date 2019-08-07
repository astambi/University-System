namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class CoursesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICourseService courseService;
        private readonly IResourceService resourceService;
        private readonly IMapper mapper;

        public CoursesController(
            UserManager<User> userManager,
            ICourseService courseService,
            IResourceService resourceService,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.courseService = courseService;
            this.resourceService = resourceService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(string search = null, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = search,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalActiveAsync(search)
            };

            var courses = await this.courseService.AllActiveWithTrainersAsync(search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = new SearchViewModel { SearchTerm = search, Placeholder = WebConstants.SearchByCourseName }
            };

            return this.View(model);
        }

        public async Task<IActionResult> Archive(string search = null, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = search,
                Action = nameof(Archive),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalArchivedAsync(search)
            };

            var courses = await this.courseService.AllArchivedWithTrainersAsync(search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = new SearchViewModel { SearchTerm = search, Placeholder = WebConstants.SearchByCourseName }
            };

            return this.View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
            => await this.UpdateEnrollment(id, false);

        // SEO friendly URL
        [Route(WebConstants.CoursesController
            + "/" + WebConstants.WithId
            + "/{name?}")]
        public async Task<IActionResult> Details(int id)
        {
            var course = await this.courseService.GetByIdAsync(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = this.mapper.Map<CourseDetailsViewModel>(course);

            var userId = this.userManager.GetUserId(this.User);
            if (userId != null)
            {
                model.IsTrainer = userId == model.TrainerId;
                model.IsUserEnrolled = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);

                if (model.IsUserEnrolled)
                {
                    model.Resources = await this.resourceService.AllByCourseAsync(id);
                }
            }

            return this.View(model);
        }

        [Authorize]
        [HttpPost]
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

            var isEnrolled = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);

            return enrollAction
                ? await this.TryEnroll(id, userId, isEnrolled)
                : await this.TryCancel(id, userId, isEnrolled);
        }
    }
}