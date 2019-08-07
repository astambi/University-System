namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Infrastructure.Helpers;
    using LearningSystem.Web.Models.Exams;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ExamsController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICourseService courseService;
        private readonly IExamService examService;

        public ExamsController(
            UserManager<User> userManager,
            ICourseService courseService,
            IExamService examService)
        {
            this.userManager = userManager;
            this.courseService = courseService;
            this.examService = examService;
        }

        [Route(WebConstants.ExamsController
            + "/course/"
            + WebConstants.WithId)]
        public async Task<IActionResult> All(int id) // courseId
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToCoursesIndex();
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var isEnrolledInCourse = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);
            if (!isEnrolledInCourse)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceDownloadUnauthorizedMsg);
                return this.RedirectToCoursesIndex();
            }

            var exams = await this.examService.AllByStudentCourseAsync(id, userId);
            var course = await this.courseService.GetBasicByIdAsync(id);

            var model = new ExamSubmissionsListingViewModel
            {
                ExamSubmissions = exams,
                Course = course
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int id, IFormFile examFile)
        {
            var course = await this.courseService.GetByIdAsync(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToCoursesIndex();
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var isEnrolled = await this.courseService.IsUserEnrolledInCourseAsync(id, userId);
            if (!isEnrolled)
            {
                this.TempData.AddErrorMessage(WebConstants.UserNotEnrolledInCourseMsg);
                return this.RedirectToCourseDetails(id);
            }

            if (!course.IsExamSubmissionDate)
            {
                this.TempData.AddErrorMessage(WebConstants.FileSubmittionDateMsg);
                return this.RedirectToCourseDetails(id);
            }

            if (examFile == null)
            {
                this.TempData.AddErrorMessage(WebConstants.FileNotSubmittedMsg);
                return this.RedirectToCourseDetails(id);
            }

            if (!examFile.FileName.EndsWith($".{DataConstants.FileType}")
                || !examFile.ContentType.Contains(DataConstants.FileType))
            {
                this.TempData.AddErrorMessage(string.Format(WebConstants.FileFormatInvalidMsg, DataConstants.FileType));
                return this.RedirectToCourseDetails(id);
            }

            if (examFile.Length == 0
                || examFile.Length > DataConstants.FileMaxLengthInBytes)
            {
                this.TempData.AddErrorMessage(string.Format(WebConstants.FileSizeInvalidMsg, DataConstants.FileMaxLengthInMb));
                return this.RedirectToCourseDetails(id);
            }

            var fileBytes = await examFile.ToByteArrayAsync();
            await this.examService.CreateAsync(id, userId, fileBytes);

            this.TempData.AddSuccessMessage(WebConstants.ExamSubmittedMsg);

            return this.RedirectToAction(nameof(All), new { id });
        }

        public async Task<IActionResult> Download(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var existsForStudent = await this.examService.ExistsForStudentAsync(id, userId);
            if (!existsForStudent)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var exam = await this.examService.DownloadAsync(id);
            if (exam == null)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentHasNotSubmittedExamMsg);
                return this.RedirectToCoursesIndex();
            }

            var fileName = FileHelpers.ExamFileName(
                exam.CourseName,
                exam.StudentUserName,
                exam.SubmissionDate);

            return this.File(exam.FileSubmission, WebConstants.ApplicationZip, fileName);
        }

        private IActionResult RedirectToCourseDetails(int id)
            => this.RedirectToAction(
                nameof(CoursesController.Details),
                WebConstants.CoursesController,
                routeValues: new { id });

        private RedirectToActionResult RedirectToCoursesIndex()
            => this.RedirectToAction(
                nameof(CoursesController.Index),
                WebConstants.CoursesController);
    }
}