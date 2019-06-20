namespace LearningSystem.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Admin;
    using LearningSystem.Web.Areas.Admin.Models.Courses;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CoursesController : BaseAdminController
    {
        private const string CourseFormView = "CourseForm";

        private readonly UserManager<User> userManager;
        private readonly IAdminCourseService adminCourseService;
        private readonly ICourseService courseService;
        private readonly IMapper mapper;

        public CoursesController(
            UserManager<User> userManager,
            IAdminCourseService adminCourseService,
            ICourseService courseService,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.adminCourseService = adminCourseService;
            this.courseService = courseService;
            this.mapper = mapper;
        }

        public IActionResult Index() => this.View();

        public async Task<IActionResult> Create()
        {
            var model = new CourseFormModel
            {
                Trainers = await this.GetTrainers(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow
            };

            return this.View(CourseFormView, model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseFormModel model)
        {
            var user = await this.userManager.FindByIdAsync(model.TrainerId);
            if (user == null)
            {
                this.ModelState.AddModelError(string.Empty, WebConstants.InvalidUserMsg);
            }

            if (!this.ModelState.IsValid)
            {
                model.Trainers = await this.GetTrainers();
                return this.View(CourseFormView, model);
            }

            await this.adminCourseService.Create(
                model.Name,
                model.Description,
                model.StartDate,
                model.EndDate,
                model.TrainerId);

            this.TempData.AddSuccessMessage(WebConstants.CourseCreatedMsg);
            return this.RedirectToAction(nameof(HomeController.Index), WebConstants.HomeController);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = this.adminCourseService.GetById(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index), WebConstants.HomeController);
            }

            var model = this.mapper.Map<CourseFormModel>(course);
            model.Trainers = await this.GetTrainers();
            model.Action = FormActionEnum.Edit;

            return this.View(CourseFormView, model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CourseFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index), WebConstants.HomeController);
            }

            var user = await this.userManager.FindByIdAsync(model.TrainerId);
            if (user == null)
            {
                this.ModelState.AddModelError(string.Empty, WebConstants.InvalidUserMsg);
            }

            if (!this.ModelState.IsValid)
            {
                model.Trainers = await this.GetTrainers();
                return this.View(CourseFormView, model);
            }

            await this.adminCourseService.UpdateAsync(
                 id,
                 model.Name,
                 model.Description,
                 model.StartDate,
                 model.EndDate,
                 model.TrainerId);

            this.TempData.AddSuccessMessage(WebConstants.CourseUpdatedMsg);
            return this.RedirectToAction(nameof(HomeController.Index), WebConstants.HomeController);
        }

        private async Task<IEnumerable<SelectListItem>> GetTrainers()
            => (await this.userManager.GetUsersInRoleAsync(WebConstants.TrainerRole))
            .Select(t => new SelectListItem
            {
                Text = $"{t.Name} ({t.UserName})",
                Value = t.Id
            })
            .OrderBy(t => t.Text)
            .ToList();
    }
}