namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models.Resources;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ResourcesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICourseService courseService;
        private readonly IResourceService resourceService;
        private readonly ITrainerService trainerService;

        public ResourcesController(
            UserManager<User> userManager,
            ICourseService courseService,
            IResourceService resourceService,
            ITrainerService trainerService)
        {
            this.userManager = userManager;
            this.courseService = courseService;
            this.resourceService = resourceService;
            this.trainerService = trainerService;
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        [HttpPost]
        public async Task<IActionResult> Create(int courseId, ResourceCreateFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.FileInvalidMsg);
                return this.RedirectToTrainersResources(courseId);
            }

            if (courseId != model.CourseId)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseInvalidMsg);
                return this.RedirectToTrainersIndex();
            }

            var courseExists = this.courseService.Exists(courseId);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToTrainersIndex();
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToTrainersIndex();
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, courseId);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToTrainersIndex();
            }

            var resourceFile = model.ResourceFile;
            var fileBytes = await resourceFile.ToByteArrayAsync();

            var success = await this.resourceService.CreateAsync(
                courseId,
                resourceFile.FileName,
                resourceFile.ContentType,
                fileBytes);

            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceFileUploadErrorMsg);
            }

            this.TempData.AddSuccessMessage(WebConstants.ResourceCreatedMsg);

            return this.RedirectToTrainersResources(courseId);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, ResourceFormModel model)
        {
            var courseId = model.CourseId;

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToTrainersIndex();
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, courseId);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToTrainersIndex();
            }

            var resourceExists = this.resourceService.Exists(id);
            if (!resourceExists)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceNotFoundMsg);
                return this.RedirectToTrainersResources(courseId);
            }

            var success = await this.resourceService.RemoveAsync(id);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceNotDeletedMsg);
            }

            this.TempData.AddSuccessMessage(WebConstants.ResourceDeletedMsg);

            return this.RedirectToTrainersResources(courseId);
        }

        public async Task<IActionResult> Download(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCoursesIndex();
            }

            var exists = this.resourceService.Exists(id);
            if (!exists)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceNotFoundMsg);
                return this.RedirectToCoursesIndex();
            }

            var canBeDownloadedByUser = await this.resourceService.CanBeDownloadedByUser(id, userId);
            if (!canBeDownloadedByUser)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceDownloadUnauthorizedMsg);
                return this.RedirectToCoursesIndex();
            }

            var resource = await this.resourceService.DownloadAsync(id);
            if (resource == null)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceDownloadErrorMsg);
                return this.RedirectToCoursesIndex();
            }

            return this.File(resource.FileBytes, resource.ContentType, resource.FileName);
        }

        private IActionResult RedirectToCoursesIndex()
            => this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);

        private IActionResult RedirectToTrainersIndex()
            => this.RedirectToAction(nameof(TrainersController.Index), WebConstants.TrainersController);

        private IActionResult RedirectToTrainersResources(int courseId)
            => this.RedirectToAction(nameof(TrainersController.Resources), WebConstants.TrainersController, routeValues: new { id = courseId });
    }
}