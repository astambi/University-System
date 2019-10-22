namespace University.Web.Controllers
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models;
    using University.Web.Models.Courses;

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

        public async Task<IActionResult> Index(string searchTerm, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = searchTerm,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalActiveAsync(searchTerm)
            };

            var search = new SearchViewModel
            {
                Controller = WebConstants.CoursesController,
                Action = nameof(Index),
                SearchTerm = searchTerm,
                Placeholder = WebConstants.SearchByCourseName
            };

            var courses = await this.courseService.AllActiveAsync(searchTerm, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = search
            };

            return this.View(model);
        }

        public async Task<IActionResult> Archive(string searchTerm, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = searchTerm,
                Action = nameof(Archive),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalArchivedAsync(searchTerm)
            };

            var search = new SearchViewModel
            {
                Controller = WebConstants.CoursesController,
                Action = nameof(Archive),
                SearchTerm = searchTerm,
                Placeholder = WebConstants.SearchByCourseName
            };

            var courses = await this.courseService.AllArchivedAsync(searchTerm, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = search
            };

            return this.View(model);
        }

        // SEO friendly URL
        [Route(WebConstants.CoursesController + "/" + WebConstants.WithId + "/{name?}")]
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
        public async Task<IActionResult> Cancel(int id)
            => await this.UpdateEnrollment(id, false);

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enroll(int id)
            => await this.UpdateEnrollment(id, true);

        private async Task<IActionResult> TryCancel(int id, string userId, bool isEnrolled)
        {
            if (!isEnrolled)
            {
                this.TempData.AddInfoMessage(WebConstants.UserNotEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            var success = await this.courseService.CancellUserEnrollmentInCourseAsync(id, userId);
            if (success)
            {
                this.TempData.AddSuccessMessage(WebConstants.CourseEnrollmentCancellationSuccessMsg);
            }

            return this.RedirectToAction(nameof(Details), routeValues: new { id });
        }

        private async Task<IActionResult> TryEnroll(int id, string userId, bool isEnrolled)
        {
            if (isEnrolled)
            {
                this.TempData.AddInfoMessage(WebConstants.UserEnrolledInCourseAlreadyMsg);
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            await this.courseService.EnrollUserInCourseAsync(id, userId);
            this.TempData.AddSuccessMessage(WebConstants.CourseEnrollmentSuccessMsg);

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