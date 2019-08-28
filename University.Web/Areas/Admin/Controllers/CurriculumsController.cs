namespace University.Web.Areas.Admin.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Common.Infrastructure.Extensions;
    using University.Services;
    using University.Services.Admin;
    using University.Web.Areas.Admin.Models.Curriculums;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models;

    public class CurriculumsController : BaseAdminController
    {
        public const string CurriculumFormView = "CurriculumForm";

        private readonly IAdminCourseService adminCourseService;
        private readonly IAdminCurriculumService adminCurriculumService;
        private readonly ICourseService courseService;
        private readonly IMapper mapper;

        public CurriculumsController(
            IAdminCourseService adminCourseService,
            IAdminCurriculumService adminCurriculumService,
            ICourseService courseService,
            IMapper mapper)
        {
            this.adminCourseService = adminCourseService;
            this.adminCurriculumService = adminCurriculumService;
            this.courseService = courseService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CurriculumListingViewModel
            {
                Curriculums = await this.adminCurriculumService.AllAsync(),
                CoursesSelectList = await this.GetCoursesSelectListItems()
            };

            return this.View(model);
        }

        public IActionResult Create()
            => this.View(CurriculumFormView, new CurriculumFormModel());

        [HttpPost]
        public async Task<IActionResult> Create(CurriculumFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(CurriculumFormView, model);
            }

            var id = await this.adminCurriculumService.CreateAsync(model.Name, model.Description);
            if (id < 0)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumCreateErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.CurriculumCreateSuccessMsg);
            }

            return this.RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
           => await this.LoadCurriculumForm(id, FormActionEnum.Edit);

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CurriculumFormModel model)
        {
            var exists = await this.adminCurriculumService.ExistsAsync(id);
            if (!exists
                || id != model.Id)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(CurriculumFormView, model);
            }

            var success = await this.adminCurriculumService.UpdateAsync(id, model.Name, model.Description);
            if (!success)
            {
                this.TempData.AddInfoMessage(WebConstants.CurriculumUpdateErrorMsg);
                return this.View(CurriculumFormView, model);
            }

            this.TempData.AddSuccessMessage(WebConstants.CurriculumUpdateSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
           => await this.LoadCurriculumForm(id, FormActionEnum.Delete);

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CurriculumFormModel model)
        {
            var exists = await this.adminCurriculumService.ExistsAsync(id);
            if (!exists
                || id != model.Id)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var success = await this.adminCurriculumService.RemoveAsync(id);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumDeleteErrorMsg);
                return this.RedirectToAction(nameof(Delete), new { id });
            }

            this.TempData.AddSuccessMessage(WebConstants.CurriculumDeleteSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(CurriculumAddRemoveCourseFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(nameof(Index));
            }

            var curriculumExists = await this.adminCurriculumService.ExistsAsync(model.CurriculumId);
            if (!curriculumExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseExists = this.courseService.Exists(model.CourseId);
            if (!curriculumExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var curriculumCourseExists = await this.adminCurriculumService.ExistsCurriculumCourseAsync(model.CurriculumId, model.CourseId);
            if (curriculumCourseExists)
            {
                this.TempData.AddInfoMessage(WebConstants.CurriculumContainsCourseAlreadyMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var success = await this.adminCurriculumService.AddCourseAsync(model.CurriculumId, model.CourseId);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseAddedToCurriculumErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.CourseAddedToCurriculumSuccessMsg);
            }

            return this.RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCourse(CurriculumAddRemoveCourseFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(nameof(Index));
            }

            var curriculumExists = await this.adminCurriculumService.ExistsAsync(model.CurriculumId);
            if (!curriculumExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseExists = this.courseService.Exists(model.CourseId);
            if (!curriculumExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var curriculumCourseExists = await this.adminCurriculumService.ExistsCurriculumCourseAsync(model.CurriculumId, model.CourseId);
            if (!curriculumCourseExists)
            {
                this.TempData.AddInfoMessage(WebConstants.CurriculumDoesNotContainCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var success = await this.adminCurriculumService.RemoveCourseAsync(model.CurriculumId, model.CourseId);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseRemoveFromCurriculumErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.CourseRemoveFromCurriculumSuccessMsg);
            }

            return this.RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Graduates(int id)
        {
            var curriculumExists = await this.adminCurriculumService.ExistsAsync(id);
            if (!curriculumExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = new CurriculumGraduatesListingViewModel
            {
                Curriculum = await this.adminCurriculumService.GetByIdAsync(id),
                Graduates = await this.adminCurriculumService.GetDiplomaGraduatesAsync(id),
                Candidates = await this.adminCurriculumService.GetEligibleCandidatesAsync(id),
            };

            return this.View(model);
        }

        private async Task<IEnumerable<SelectListItem>> GetCoursesSelectListItems()
            => (await this.adminCourseService.AllAsync())
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name} - {c.StartDate.ToDateString()}"
            })
            .ToList();

        private async Task<IActionResult> LoadCurriculumForm(int id, FormActionEnum action)
        {
            var curriculum = await this.adminCurriculumService.GetByIdAsync(id);
            if (curriculum == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = this.mapper.Map<CurriculumFormModel>(curriculum);
            model.Action = action;

            return this.View(CurriculumFormView, model);
        }
    }
}