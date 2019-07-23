namespace LearningSystem.Tests.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Resources;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models.Resources;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class ResourcesControllerTest
    {
        private const int TestCourseId = 1;
        private const int TestResourceId = 10;
        private const string TestUserId = "TestUserId";
        private const string TestContentType = "application/zip";
        private const string TestFileName = "TestFileName";

        private readonly byte[] TestFileBytes = new byte[] { 158, 201, 3, 7 };

        [Fact]
        public void ResourcesController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var authorizeAttribute = typeof(ResourcesController)
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Create_ShouldBeForAuthorizedTrainersOnly()
            => this.AssertAuthorizeAttributeForRoleTrainer(nameof(ResourcesController.Create));

        [Fact]
        public void Create_ShouldHaveHpptPostAttribute()
            => this.AssertHttpPostAttribute(nameof(ResourcesController.Create));

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResources_GivenInvalidFormModel()
        {
            // Arrange
            var controller = new ResourcesController(
                userManager: null,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };
            controller.ModelState.AddModelError(string.Empty, "Error");

            // Act
            var result = await controller.Create(TestCourseId, null);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceNotFoundMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidCourse()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new ResourcesController(
                userManager: null,
                courseService.Object,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            var testIFormFile = IFormFileMock.GetMock;

            // Act
            var result = await controller.Create(TestCourseId, testIFormFile.Object);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToTrainersIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidUser()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            var testIFormFile = IFormFileMock.GetMock;

            // Act
            var result = await controller.Create(TestCourseId, testIFormFile.Object);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToTrainersIndex(result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidTrainer()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            var testIFormFile = IFormFileMock.GetMock;

            // Act
            var result = await controller.Create(TestCourseId, testIFormFile.Object);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersIndex(result);

            courseService.Verify();
            userManager.Verify();
            trainerService.Verify();
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResourcesWithErrorMsg_GivenCreateError()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.CreateAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            var testIFormFile = IFormFileMock.GetMock;

            // Act
            var result = await controller.Create(TestCourseId, testIFormFile.Object);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceFileUploadErrorMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
            userManager.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResourcesWithSuccessMsg_GivenCreateSuccess()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.CreateAsync(true);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            var testIFormFile = IFormFileMock.GetMock;

            // Act
            var result = await controller.Create(TestCourseId, testIFormFile.Object);

            // Assert
            controller.TempData.AssertSuccessMsg(WebConstants.ResourceCreatedMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
            userManager.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Fact]
        public void Delete_ShouldBeForAuthorizedTrainersOnly()
            => this.AssertAuthorizeAttributeForRoleTrainer(nameof(ResourcesController.Delete));

        [Fact]
        public void Delete_ShouldHaveHpptPostAttribute()
            => this.AssertHttpPostAttribute(nameof(ResourcesController.Delete));

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersIndex_GivenInvalidUser()
        {
            // Arrange
            var testModel = new ResourceFormViewModel { CourseId = TestCourseId, Id = TestResourceId };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestResourceId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToTrainersIndex(result);

            userManager.Verify();
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersIndex_GivenInvalidTrainer()
        {
            // Arrange
            var testModel = new ResourceFormViewModel { CourseId = TestCourseId, Id = TestResourceId };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersIndex(result);

            userManager.Verify();
            trainerService.Verify();
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResources_GivenInvalidResource()
        {
            // Arrange
            var testModel = new ResourceFormViewModel { CourseId = TestCourseId, Id = TestResourceId };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.Exists(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceNotFoundMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResourcesWithErrorMsg_GivenRemoveError()
        {
            // Arrange
            var testModel = new ResourceFormViewModel { CourseId = TestCourseId, Id = TestResourceId };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .RemoveAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceNotDeletedMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResourcesWithSuccessMsg_GivenRemoveSuccess()
        {
            // Arrange
            var testModel = new ResourceFormViewModel { CourseId = TestCourseId, Id = TestResourceId };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .RemoveAsync(true);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertSuccessMsg(WebConstants.ResourceDeletedMsg);

            this.AssertRedirectToTrainersResources(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseDetails_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Download(TestResourceId, TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToCourseDetails(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseDetails_GivenInvalidTrainerOrStudent()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(false);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Download(TestResourceId, TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceDownloadUnauthorizedMsg);

            this.AssertRedirectToCourseDetails(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
            courseService.Verify();
            trainerService.Verify();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Download_ShouldRedirectToCourseDetailsWithErrorMsg_GivenDownloadError(bool trainerOrStudentEnrolled)
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(trainerOrStudentEnrolled); // either trainer or enrolled student

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(!trainerOrStudentEnrolled); // either trainer or enrolled student

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.DownloadAsync(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Download(TestResourceId, TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ResourceNotFoundMsg);

            this.AssertRedirectToCourseDetails(result);
            this.AssertRouteWithId(result);

            userManager.Verify();
            courseService.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Download_ShouldRedirectToCourseDetailsWithSuccessMsg_GivenDownloadSuccess(bool trainerOrStudentEnrolled)
        {
            // Arrange
            var testResourceDload = new ResourceDownloadServiceModel
            {
                FileName = TestFileName,
                ContentType = TestContentType,
                FileBytes = TestFileBytes
            };

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(trainerOrStudentEnrolled); // either trainer or enrolled student

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(!trainerOrStudentEnrolled); // either trainer or enrolled student

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.DownloadAsync(testResourceDload);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object);

            // Act
            var result = await controller.Download(TestResourceId, TestCourseId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(this.TestFileBytes, fileContentResult.FileContents);
            Assert.Equal(TestContentType, fileContentResult.ContentType);
            Assert.Equal(TestFileName, fileContentResult.FileDownloadName);

            userManager.Verify();
            courseService.Verify();
            trainerService.Verify();
            resourceService.Verify();
        }

        private void AssertAuthorizeAttributeForRoleTrainer(string methodName)
        {
            // Act
            var method = typeof(ResourcesController).GetMethod(methodName);

            var authorizeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(WebConstants.TrainerRole, authorizeAttribute.Roles);
        }

        private void AssertHttpPostAttribute(string methodName)
        {
            // Act
            var method = typeof(ResourcesController).GetMethod(methodName);

            var httpPostAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(HttpPostAttribute))
                as HttpPostAttribute;

            // Assert
            Assert.NotNull(httpPostAttribute);
        }

        private RedirectToActionResult AssertRedirectToAction(IActionResult result)
            => Assert.IsType<RedirectToActionResult>(result);

        private void AssertRedirectToCourseDetails(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(CoursesController.Details), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);
        }

        private void AssertRedirectToTrainersIndex(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(TrainersController.Index), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.TrainersController, redirectToActionResult.ControllerName);
        }

        private void AssertRedirectToTrainersResources(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(TrainersController.Resources), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.TrainersController, redirectToActionResult.ControllerName);
        }

        private void AssertRouteWithId(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.Contains(redirectToActionResult.RouteValues.Keys, k => k == WebConstants.Id);
            Assert.Equal(TestCourseId, redirectToActionResult.RouteValues[WebConstants.Id]);
        }
    }
}
