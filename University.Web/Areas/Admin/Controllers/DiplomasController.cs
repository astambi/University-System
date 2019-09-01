namespace University.Web.Areas.Admin.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using University.Services.Admin;
    using University.Web.Areas.Admin.Models.Diplomas;
    using University.Web.Infrastructure.Extensions;

    public class DiplomasController : BaseAdminController
    {
        private readonly IAdminCurriculumService adminCurriculumService;
        private readonly IAdminDiplomaService adminDiplomaService;
        private readonly IMemoryCache cache;

        public DiplomasController(
            IAdminCurriculumService adminCurriculumService,
            IAdminDiplomaService adminDiplomaService,
            IMemoryCache cache)
        {
            this.adminCurriculumService = adminCurriculumService;
            this.adminDiplomaService = adminDiplomaService;
            this.cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int id, DiplomaCreateFormModel model)
        {
            var curriculumExists = await this.adminCurriculumService.ExistsAsync(id);
            if (!curriculumExists
                || id != model.CurriculumId)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumNotFoundMsg);
                return this.RedirectToCurriculums();
            }

            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.CurriculumOrStudentErrorMsg);
                return this.RedirectToCurriculumGraduates(id);
            }

            var hasPassedAllCurriculumCourses = await this.adminDiplomaService.HasPassedAllCurriculumCoursesAsync(model.CurriculumId, model.StudentId);
            if (!hasPassedAllCurriculumCourses)
            {
                this.TempData.AddInfoMessage(WebConstants.StudentNotEligibleForDiplomaMsg);
                return this.RedirectToCurriculumGraduates(id);
            }

            var diplomaExists = await this.adminDiplomaService.ExistsForCurriculumStudentAsync(model.CurriculumId, model.StudentId);
            if (diplomaExists)
            {
                this.TempData.AddInfoMessage(WebConstants.DiplomaExistsAlreadyMsg);
                return this.RedirectToCurriculumGraduates(id);
            }

            var success = await this.adminDiplomaService.CreateAsync(model.CurriculumId, model.StudentId);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.DiplomaCreateErrorMsg);
            }
            else
            {
                // Clear curriculum cache (candidate & graduates)
                this.cache.Clear(model.CurriculumId);

                this.TempData.AddSuccessMessage(WebConstants.DiplomaCreateSuccessMsg);
            }

            return this.RedirectToCurriculumGraduates(id);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, DiplomaDeleteFormModel model)
        {
            var diplomaExists = await this.adminDiplomaService.ExistsAsync(id);
            if (!diplomaExists
                || model == null
                || id != model.DiplomaId)
            {
                this.TempData.AddErrorMessage(WebConstants.DiplomaNotFoundMsg);
                if (model == null)
                {
                    return this.RedirectToCurriculums();
                }

                return this.RedirectToCurriculumGraduates(model.CurriculumId);
            }

            var success = await this.adminDiplomaService.RemoveAsync(model.DiplomaId);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.DiplomaDeleteErrorMsg);
            }
            else
            {
                // Clear curriculum cache (candidate & graduates)
                this.cache.Clear(model.CurriculumId);

                this.TempData.AddSuccessMessage(WebConstants.DiplomaDeleteSuccessMsg);
            }

            return this.RedirectToCurriculumGraduates(model.CurriculumId);
        }

        private IActionResult RedirectToCurriculums()
            => this.RedirectToAction(nameof(CurriculumsController.Index), WebConstants.CurriculumsController);

        private IActionResult RedirectToCurriculumGraduates(int id)
            => this.RedirectToAction(nameof(CurriculumsController.Graduates), WebConstants.CurriculumsController, new { id });
    }
}