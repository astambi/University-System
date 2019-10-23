namespace University.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models;
    using University.Web.Models.Courses;
    using University.Web.Models.Trainers;

    [Authorize]
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

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Courses(string searchTerm, int currentPage = 1)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);
            }

            var model = await this.GetTrainerCoursesWithSearchAndPagination(userId, searchTerm, currentPage, nameof(Courses));

            return this.View(model);
        }

        public async Task<IActionResult> Details(string id, string searchTerm, int currentPage = 1) // id = trainer username
        {
            var trainerUsername = id;

            var trainerId = await this.GetTrainerId(trainerUsername);
            if (trainerId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.TrainerNotFoundMsg);
                return this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);
            }

            var courses = await this.GetTrainerCoursesWithSearchAndPagination(trainerId, searchTerm, currentPage, nameof(Details));
            var profile = await this.trainerService.GetProfileAsync(trainerId);

            var model = new TrainerDetailsViewModel { Courses = courses, Trainer = profile };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Resources(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var model = await this.trainerService.CourseWithResourcesByIdAsync(userId, id);

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Students(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var model = new StudentCourseGradeViewModel
            {
                Course = await this.trainerService.CourseByIdAsync(userId, id),
                Students = await this.trainerService.StudentsInCourseAsync(id)
            };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        [HttpPost]
        public async Task<IActionResult> EvaluateExam(int id, StudentCourseGradeFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.GradeInvalidMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var courseHasEnded = await this.trainerService.CourseHasEndedAsync(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var isStudentInCourse = await this.courseService.IsUserEnrolledInCourseAsync(model.CourseId, model.StudentId);
            if (!isStudentInCourse)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentNotEnrolledInCourseMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var gradeValue = model.GradeBg.Value;
            var success = await this.examService.EvaluateAsync(userId, id, model.StudentId, gradeValue);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamAssessmentErrorMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            this.TempData.AddSuccessMessage(WebConstants.ExamAssessedMsg);
            await this.CreateCertificate(id, userId, model);

            return this.RedirectToTrainerStudentsForCourse(id);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        [HttpPost]
        public async Task<IActionResult> DeleteCertificate(int id, CertificateDeleteFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists
                || model.CourseId != id)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Courses));
            }

            var success = await this.certificateService.RemoveAsync(model.CertificateId, userId, id);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateDeletedErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.CertificateDeletedSuccessMsg);
            }

            return this.RedirectToTrainerStudentsForCourse(id);
        }

        private async Task CreateCertificate(int courseId, string trainerId, StudentCourseGradeFormModel model)
        {
            if (this.certificateService.IsGradeEligibleForCertificate(model.GradeBg))
            {
                var success = await this.certificateService.CreateAsync(trainerId, courseId, model.StudentId, model.GradeBg.Value);
                if (success)
                {
                    this.TempData.AddSuccessMessage(WebConstants.ExamAssessedMsg + Environment.NewLine + WebConstants.CertificateIssuedMsg);
                }
            }
        }

        private async Task<CoursePageListingViewModel> GetTrainerCoursesWithSearchAndPagination(
            string trainerId, string searchTerm, int currentPage, string action = nameof(Courses))
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = searchTerm,
                Action = action,
                RequestedPage = currentPage,
                TotalItems = await this.trainerService.TotalCoursesAsync(trainerId, searchTerm)
            };

            var search = new SearchViewModel
            {
                Controller = WebConstants.TrainersController,
                Action = action,
                SearchTerm = searchTerm,
                Placeholder = WebConstants.SearchByCourseName
            };

            var courses = await this.trainerService.CoursesAsync(trainerId, searchTerm, pagination.CurrentPage, WebConstants.PageSize);
            var coursesToEvaluate = await this.trainerService.CoursesToEvaluateAsync(trainerId);

            var model = new TrainerCoursePageListingViewModel
            {
                CoursesToEvaluate = coursesToEvaluate,
                Courses = courses,
                Pagination = pagination,
                Search = search
            };

            return model;
        }

        private async Task<string> GetTrainerId(string trainerUsername)
            => (await this.userManager.GetUsersInRoleAsync(WebConstants.TrainerRole))
            .Where(u => u.UserName == trainerUsername)
            .Select(u => u.Id)
            .FirstOrDefault();

        private IActionResult RedirectToTrainerStudentsForCourse(int courseId)
            => this.RedirectToAction(nameof(Students), routeValues: new { id = courseId });
    }
}