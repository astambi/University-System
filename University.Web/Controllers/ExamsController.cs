namespace University.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models.Exams;

    [Authorize]
    public class ExamsController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICloudinaryService cloudinaryService;
        private readonly ICourseService courseService;
        private readonly IExamService examService;

        public ExamsController(
            UserManager<User> userManager,
            ICloudinaryService cloudinaryService,
            ICourseService courseService,
            IExamService examService)
        {
            this.userManager = userManager;
            this.cloudinaryService = cloudinaryService;
            this.courseService = courseService;
            this.examService = examService;
        }

        public async Task<IActionResult> Course(int id)
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
            var course = await this.courseService.GetByIdBasicAsync(id);

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
            var fileName = $"{this.User.Identity.Name}-{examFile.FileName}";

            var fileUrl = this.cloudinaryService.UploadFile(fileBytes, fileName, WebConstants.CloudExamsFolder);

            var success = await this.examService.CreateAsync(id, userId, fileName, fileUrl);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamSubmitErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.ExamSubmittedMsg);
            }

            return this.RedirectToAction(nameof(Course), new { id });
        }

        public async Task<IActionResult> Download(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var canBeDownloadedByUser = await this.examService.CanBeDownloadedByUserAsync(id, userId);
            if (!canBeDownloadedByUser)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamDownloadUnauthorizedMsg);
                return this.RedirectToCoursesIndex();
            }

            var examUrl = await this.examService.GetDownloadUrlAsync(id);
            if (examUrl == null)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamNotFoundMsg);
                return this.RedirectToCoursesIndex();
            }

            return this.Redirect(examUrl);
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