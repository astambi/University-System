namespace University.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Data.Models;
    using University.Services;
    using University.Services.Admin;
    using University.Web.Areas.Admin.Models.Courses;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models;

    public class CoursesController : BaseAdminController
    {
        public const string CourseFormView = "CourseForm";

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

        public async Task<IActionResult> Create()
        {
            var model = new CourseFormModel
            {
                Trainers = await this.GetTrainersAsync(),
                StartDate = DateTime.Now, // local time
                EndDate = DateTime.Now    // local time
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
                model.Trainers = await this.GetTrainersAsync();
                return this.View(CourseFormView, model);
            }

            var id = await this.adminCourseService.CreateAsync(
                model.Name,
                model.Description,
                model.StartDate,
                model.EndDate,
                model.Price,
                model.TrainerId);

            if (id < 0)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseCreateErrorMsg);

                model.Trainers = await this.GetTrainersAsync();
                return this.View(CourseFormView, model);
            }

            this.TempData.AddSuccessMessage(WebConstants.CourseCreateSuccessMsg);

            return this.RedirectToAction(nameof(Web.Controllers.CoursesController.Index), WebConstants.CoursesController);
        }

        public async Task<IActionResult> Edit(int id)
            => await this.LoadCourseForm(id, FormActionEnum.Edit);

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CourseFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Web.Controllers.CoursesController.Index), WebConstants.CoursesController);
            }

            var user = await this.userManager.FindByIdAsync(model.TrainerId);
            if (user == null)
            {
                this.ModelState.AddModelError(string.Empty, WebConstants.InvalidUserMsg);
            }

            if (!this.ModelState.IsValid)
            {
                model.Trainers = await this.GetTrainersAsync();
                return this.View(CourseFormView, model);
            }

            var success = await this.adminCourseService.UpdateAsync(
                 id,
                 model.Name,
                 model.Description,
                 model.StartDate,
                 model.EndDate,
                 model.Price,
                 model.TrainerId);

            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseUpdateErrorMsg);

                model.Trainers = await this.GetTrainersAsync();
                return this.View(CourseFormView, model);
            }

            this.TempData.AddSuccessMessage(WebConstants.CourseUpdateSuccessMsg);

            return this.RedirectToAction(
                nameof(Web.Controllers.CoursesController.Details),
                WebConstants.CoursesController,
                routeValues: new { id });
        }

        public async Task<IActionResult> Delete(int id)
            => await this.LoadCourseForm(id, FormActionEnum.Delete);

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CourseFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Web.Controllers.CoursesController.Index), WebConstants.CoursesController);
            }
            var success = await this.adminCourseService.RemoveAsync(id);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseDeleteErrorMsg);
                return this.RedirectToAction(nameof(Delete), new { id });
            }

            this.TempData.AddSuccessMessage(WebConstants.CourseDeleteSuccessMsg);

            return this.RedirectToAction(nameof(Web.Controllers.CoursesController.Index), WebConstants.CoursesController);
        }

        private async Task<IEnumerable<SelectListItem>> GetTrainersAsync()
            => (await this.userManager.GetUsersInRoleAsync(WebConstants.TrainerRole))
            .Select(t => new SelectListItem
            {
                Text = $"{t.Name} ({t.UserName})",
                Value = t.Id
            })
            .OrderBy(t => t.Text)
            .ToList();

        private async Task<IActionResult> LoadCourseForm(int id, FormActionEnum action)
        {
            var course = await this.adminCourseService.GetByIdAsync(id);
            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Web.Controllers.CoursesController.Index), WebConstants.CoursesController);
            }

            var model = this.mapper.Map<CourseFormModel>(course);
            model.Trainers = await this.GetTrainersAsync();
            model.Action = action;
            model.StartDate = model.StartDate.ToLocalTime();
            model.EndDate = model.EndDate.ToLocalTime();

            return this.View(CourseFormView, model);
        }
    }
}