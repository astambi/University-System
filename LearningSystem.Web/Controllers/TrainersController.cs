namespace LearningSystem.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Infrastructure.Helpers;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using LearningSystem.Web.Models.Trainers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = WebConstants.TrainerRole)]
    public class TrainersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICertificateService certificateService;
        private readonly ICourseService courseService;
        private readonly IExamService examService;
        private readonly ITrainerService trainerService;

        public TrainersController(
            UserManager<User> userManager,
            ICertificateService certificateService,
            ICourseService courseService,
            IExamService examService,
            ITrainerService trainerService)
        {
            this.userManager = userManager;
            this.certificateService = certificateService;
            this.courseService = courseService;
            this.examService = examService;
            this.trainerService = trainerService;
        }

        public async Task<IActionResult> Index(string search = null, int currentPage = 1)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index));
            }

            var pagination = new PaginationViewModel
            {
                SearchTerm = search,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.trainerService.TotalCoursesAsync(userId, search)
            };

            var courses = await this.trainerService.CoursesAsync(userId, search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = new SearchViewModel { SearchTerm = search, Placeholder = WebConstants.SearchByCourseName }
            };

            return this.View(model);
        }

        public async Task<IActionResult> Resources(int id)
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

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = await this.trainerService.CourseWithResourcesByIdAsync(userId, id);

            return this.View(model);
        }

        public async Task<IActionResult> Students(int id)
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

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = new StudentCourseGradeViewModel
            {
                Course = await this.trainerService.CourseByIdAsync(userId, id),
                Students = await this.trainerService.StudentsInCourseAsync(id)
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssessExam(int id, StudentCourseGradeFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentAssessmentErrorMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseHasEnded = await this.trainerService.CourseHasEndedAsync(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var isStudentInCourse = await this.courseService.IsUserEnrolledInCourseAsync(model.CourseId, model.StudentId);
            if (!isStudentInCourse)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentNotEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var gradeValue = model.Grade.Value;
            var assessmentSuccess = await this.examService.EvaluateAsync(userId, id, model.StudentId, gradeValue);
            if (!assessmentSuccess)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamAssessmentErrorMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            this.TempData.AddSuccessMessage(WebConstants.ExamAssessedMsg);

            // Issue new certificate
            if (this.certificateService.IsGradeEligibleForCertificate(model.Grade))
            {
                var success = await this.certificateService.CreateAsync(userId, id, model.StudentId, model.Grade.Value);
                if (success)
                {
                    this.TempData.AddSuccessMessage(
                        WebConstants.ExamAssessedMsg
                        + Environment.NewLine
                        + WebConstants.CertificateIssuedMsg);
                }
            }

            return this.RedirectToAction(nameof(Students), routeValues: new { id });
        }

        public async Task<IActionResult> DownloadExam(int id, string studentId)
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

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseHasEnded = await this.trainerService.CourseHasEndedAsync(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var exam = await this.examService.DownloadForTrainerAsync(userId, id, studentId);
            if (exam == null)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentHasNotSubmittedExamMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var fileName = FileHelpers.ExamFileName(exam.CourseName, exam.StudentUserName, exam.SubmissionDate);

            return this.File(exam.FileSubmission, WebConstants.ApplicationZip, fileName);
        }
    }
}