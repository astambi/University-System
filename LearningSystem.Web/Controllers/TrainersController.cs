﻿namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models.Trainers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = WebConstants.TrainerRole)]
    public class TrainersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICourseService courseService;
        private readonly ITrainerService trainerService;

        public TrainersController(
            UserManager<User> userManager,
            ICourseService courseService,
            ITrainerService trainerService)
        {
            this.userManager = userManager;
            this.courseService = courseService;
            this.trainerService = trainerService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index));
            }

            var courses = await this.trainerService.CoursesAsync(userId);

            return this.View(courses);
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
                this.TempData.AddErrorMessage(WebConstants.NotCourseTrainerMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = new StudentCourseGradeViewModel
            {
                Course = await this.trainerService.CourseAsync(userId, id),
                Students = await this.trainerService.StudentsInCourseAsync(id)
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssessPerformance(int id, StudentCourseGradeFormModel model)
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
                this.TempData.AddErrorMessage(WebConstants.NotCourseTrainerMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseHasEnded = await this.trainerService.CourseHasEnded(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            var isStudentInCourse = await this.courseService.UserIsEnrolledInCourseAsync(model.CourseId, model.StudentId);
            if (!isStudentInCourse)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentNotEnrolledInCourseMsg);
                return this.RedirectToAction(nameof(Students), routeValues: new { id });
            }

            await this.trainerService.AssessStudentCoursePerformance(
                userId,
                id,
                model.StudentId,
                (Grade)model.Grade);

            return this.RedirectToAction(nameof(Students), routeValues: new { id });
        }
    }
}