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
        private readonly IMapper mapper;

        public CoursesController(
            UserManager<User> userManager,
            ICourseService courseService,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.courseService = courseService;
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
        [Route(WebConstants.CoursesController + "/{id}/{name?}")]
        public async Task<IActionResult> Details(int id)
        {
            var course = await this.courseService.GetByIdAsync(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId != null)
            {
                course.IsUserEnrolled = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);
            }

            return this.View(course);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enroll(int id)
            => await this.UpdateEnrollment(id, true);

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadExam(int id, IFormFile examFile)
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

            var isEnrolled = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);
            if (!isEnrolled)
            {
                this.TempData.AddErrorMessage(WebConstants.UserNotEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            if (!course.IsExamSubmissionDate)
            {
                this.TempData.AddErrorMessage(WebConstants.FileSubmittionDateMsg);
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            if (examFile == null)
            {
                this.TempData.AddErrorMessage(WebConstants.FileNotSubmittedMsg);
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            if (!examFile.FileName.EndsWith($".{DataConstants.FileType}")
                || !examFile.ContentType.Contains(DataConstants.FileType))
            {
                this.TempData.AddErrorMessage(string.Format(WebConstants.FileFormatInvalidMsg, DataConstants.FileType));
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            if (examFile.Length == 0
                || examFile.Length > DataConstants.FileMaxLengthInBytes)
            {
                this.TempData.AddErrorMessage(string.Format(WebConstants.FileSizeInvalidMsg, DataConstants.FileMaxLengthInMb));
                return this.RedirectToAction(nameof(Details), routeValues: new { id });
            }

            var fileBytes = await examFile.ToByteArray();
            await this.courseService.AddExamSubmission(id, userId, fileBytes);

            this.TempData.AddSuccessMessage(WebConstants.ExamSubmittedMsg);

            return this.RedirectToAction(nameof(Details), routeValues: new { id });
        }

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