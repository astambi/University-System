namespace LearningSystem.Tests.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Admin;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Areas.Admin.Controllers;
    using LearningSystem.Web.Areas.Admin.Models.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Moq;
    using Xunit;

    public class CoursesControllerTest
    {
        private const string FirstUserId = "1";
        private const string SecondUserId = "2";
        private const string ThirdUserId = "3";

        private const string FirstUserName = "B";
        private const string SecondUserName = "A";
        private const string ThirdUserName = "A";

        private const string FirstUserUsername = "A";
        private const string SecondUserUsernname = "A";
        private const string ThirdUserUsername = "B";

        [Fact]
        public void CoursesController_ShouldBeInAdminArea()
        {
            // Arrange
            var attributes = typeof(CoursesController).GetCustomAttributes(true);

            // Act
            var areaAttribute = attributes
                .FirstOrDefault(a => a.GetType() == typeof(AreaAttribute))
                as AreaAttribute;

            // Assert
            Assert.NotNull(areaAttribute);
            Assert.Equal(WebConstants.AdminArea, areaAttribute.RouteValue);
        }

        [Fact]
        public void CoursesController_ShouldBeAuthorizedForAdminRoleOnly()
        {
            // Arrange
            var attributes = typeof(CoursesController).GetCustomAttributes(true);

            // Act
            var authorizeAttribute = attributes
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(WebConstants.AdministratorRole, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task CreateGet_ShouldReturnViewWithCorrectModel()
        {
            // Arrange
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            UserManagerMock.GetUsersInRoleAsync(userManager, trainers);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null, courseService: null, mapper: null);

            // Act
            var result = await controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(CoursesController.CourseFormView, viewResult.ViewName);

            var model = Assert.IsType<CourseFormModel>(viewResult.Model);
            Assert.NotNull(model);
            this.AssertEqualTrainersSelectList(trainers, model.Trainers);
            this.AssertEqualDate(DateTime.Now, model.StartDate);
            this.AssertEqualDate(DateTime.Now, model.EndDate);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenInvalidTrainer()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            UserManagerMock.GetUsersInRoleAsync(userManager, trainers);
            userManager
               .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
               .ReturnsAsync((User)null) // invalid user
               .Verifiable();

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null, courseService: null, mapper: null);

            // Act
            var result = await controller.Create(form);

            // Assert
            this.AssertEqualViewResultWithModel(form, trainers, result);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenModelError()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();
            string modelTrainerId = null;

            var userManager = UserManagerMock.GetMock;
            UserManagerMock.GetUsersInRoleAsync(userManager, trainers);
            userManager
               .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
               .Callback((string trainerId) => modelTrainerId = trainerId) // service input
               .ReturnsAsync(new User() { Id = form.TrainerId }) // valid user
               .Verifiable();

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null, courseService: null, mapper: null);

            controller.ModelState.AddModelError(string.Empty, "Error"); // model error

            // Act
            var result = await controller.Create(form);

            // Assert
            Assert.Equal(form.TrainerId, modelTrainerId); // correct service model input

            this.AssertEqualViewResultWithModel(form, trainers, result);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenValidModelAndServiceError()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            UserManagerMock.GetUsersInRoleAsync(userManager, trainers);
            userManager
               .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
               .ReturnsAsync(new User() { Id = form.TrainerId }) // valid user
               .Verifiable();

            var adminCourseService = new Mock<IAdminCourseService>();
            adminCourseService
                .Setup(a => a.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(int.MinValue) // service error
                .Verifiable();

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = Tests.GetTempDataDictionary()
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            // TempData Error
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataErrorMessageKey);
            Assert.Equal(WebConstants.CourseNotCreatedMsg, controller.TempData[WebConstants.TempDataErrorMessageKey]);

            this.AssertEqualViewResultWithModel(form, trainers, result);

            userManager.Verify();
            adminCourseService.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldRedirectToAction_GivenValidModelAndServiceSuccess()
        {
            // Arrange
            var form = this.GetCreateForm();

            string modelName = null;
            string modelDescription = null;
            string modelTrainerId = null;
            var modelStartDate = DateTime.UtcNow.AddDays(-120);
            var modelEndDate = DateTime.UtcNow.AddDays(-120);

            var userManager = UserManagerMock.GetMock;
            userManager
               .Setup(u => u.FindByIdAsync(form.TrainerId))
               .ReturnsAsync(new User() { Id = form.TrainerId })
               .Verifiable();

            var adminCourseService = new Mock<IAdminCourseService>();
            adminCourseService
                .Setup(a => a.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback((string name, string description, DateTime startDate, DateTime endDate, string trainerId) =>
                {
                    modelName = name;
                    modelDescription = description;
                    modelStartDate = startDate;
                    modelEndDate = endDate;
                    modelTrainerId = trainerId;
                }) // service model input
                .ReturnsAsync(100) // service success, saved courseId
                .Verifiable();

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = Tests.GetTempDataDictionary()
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            // Service Model Input
            Assert.Equal(form.Name, modelName);
            Assert.Equal(form.Description, modelDescription);
            Assert.Equal(form.StartDate, modelStartDate);
            Assert.Equal(form.EndDate, modelEndDate);
            Assert.Equal(form.TrainerId, modelTrainerId);

            // TempData Success
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataSuccessMessageKey);
            Assert.Equal(
                WebConstants.CourseCreatedMsg,
                controller.TempData[WebConstants.TempDataSuccessMessageKey]);

            // ActionResult
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectToActionResult);

            Assert.Equal(
                nameof(LearningSystem.Web.Controllers.CoursesController.Index),
                redirectToActionResult.ActionName);

            Assert.Equal(
                WebConstants.CoursesController,
                redirectToActionResult.ControllerName);

            userManager.Verify();
            adminCourseService.Verify();
        }

        private void AssertEqualViewResultWithModel(CourseFormModel form, IList<User> trainers, IActionResult result)
        {
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(CoursesController.CourseFormView, viewResult.ViewName);

            var model = Assert.IsType<CourseFormModel>(viewResult.Model);
            Assert.NotNull(model);

            this.AssertEqualTrainersSelectList(trainers, model.Trainers);
            Assert.Equal(form, model);
        }

        private void AssertEqualTrainersSelectList(IList<User> expectedTrainers, IEnumerable<SelectListItem> resultTrainers)
        {
            Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(resultTrainers);

            Assert.Equal(expectedTrainers.Count, resultTrainers.Count());
            Assert.Equal(
                new List<string> { SecondUserId, ThirdUserId, FirstUserId },
                resultTrainers.Select(t => t.Value).ToList());

            Assert.Equal(
                new List<string>
                {
                    $"{SecondUserName} ({SecondUserUsernname})",
                    $"{ThirdUserName} ({ThirdUserUsername})",
                    $"{FirstUserName} ({FirstUserUsername})",
                },
                resultTrainers.Select(t => t.Text).ToList());
        }

        private void AssertEqualDate(DateTime expectedDate, DateTime resultDate)
        {
            Assert.Equal(expectedDate.Year, resultDate.Year);
            Assert.Equal(expectedDate.Month, resultDate.Month);
            Assert.Equal(expectedDate.Day, resultDate.Day);
        }

        private CourseFormModel GetCreateForm()
            => new CourseFormModel()
            {
                Name = "Name",
                Description = "Description",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(15),
                TrainerId = "userId"
            };

        private IList<User> GetTrainers()
            => new List<User>()
            {
                new User { Id = FirstUserId, Name = FirstUserName, UserName = FirstUserUsername}, // 3
                new User { Id = SecondUserId, Name = SecondUserName, UserName = SecondUserUsernname }, // 1
                new User { Id = ThirdUserId, Name = ThirdUserName, UserName = ThirdUserUsername }, // 2
            };
    }
}