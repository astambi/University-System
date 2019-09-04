namespace University.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models.Resources;

    [Authorize]
    public class ResourcesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICloudinaryService cloudinaryService;
        private readonly ICourseService courseService;
        private readonly IResourceService resourceService;
        private readonly ITrainerService trainerService;

        public ResourcesController(
            UserManager<User> userManager,
            ICloudinaryService cloudinaryService,
            ICourseService courseService,
            IResourceService resourceService,
            ITrainerService trainerService)
        {
            this.userManager = userManager;
            this.cloudinaryService = cloudinaryService;
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

            var fileUploadUrl = this.cloudinaryService.UploadFile(fileBytes, resourceFile.FileName, WebConstants.CloudResourcesFolder);
            var success = await this.resourceService.CreateAsync(courseId, resourceFile.FileName, fileUploadUrl);

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
            if (!resourceExists
                || id != model.ResourceId)
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

            var canBeDownloadedByUser = await this.resourceService.CanBeDownloadedByUserAsync(id, userId);
            if (!canBeDownloadedByUser)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceDownloadUnauthorizedMsg);
                return this.RedirectToCoursesIndex();
            }

            var resourceUrl = await this.resourceService.GetDownloadUrlAsync(id);
            if (resourceUrl == null)
            {
                this.TempData.AddErrorMessage(WebConstants.ResourceNotFoundMsg);
                return this.RedirectToCoursesIndex();
            }

            return this.Redirect(resourceUrl);
        }

        private IActionResult RedirectToCoursesIndex()
            => this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);

        private IActionResult RedirectToTrainersIndex()
            => this.RedirectToAction(nameof(TrainersController.Courses), WebConstants.TrainersController);

        private IActionResult RedirectToTrainersResources(int courseId)
            => this.RedirectToAction(nameof(TrainersController.Resources), WebConstants.TrainersController, routeValues: new { id = courseId });
    }
}