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
            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(this.GetTrainers());

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
            this.AssertTrainersSelectList(model.Trainers);
            this.AssertDate(model.StartDate);
            this.AssertDate(model.EndDate);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenInvalidTrainer()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(trainers);
            userManager.FindByIdAsync(null);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null, courseService: null, mapper: null);

            // Act
            var result = await controller.Create(form);

            // Assert
            this.AssertEqualViewResultWithModel(form, result);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenModelError()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(trainers);
            userManager.FindByIdAsync(new User() { Id = form.TrainerId });

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null, courseService: null, mapper: null);

            controller.ModelState.AddModelError(string.Empty, "Error"); // model error

            // Act
            var result = await controller.Create(form);

            // Assert
            this.AssertEqualViewResultWithModel(form, result);

            userManager.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldReturnViewWithCorrectModel_GivenValidModelAndServiceError()
        {
            // Arrange
            var form = this.GetCreateForm();
            var trainers = this.GetTrainers();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(trainers);
            userManager.FindByIdAsync(new User() { Id = form.TrainerId });

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.CreateAsync(int.MinValue);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotCreatedMsg);

            this.AssertEqualViewResultWithModel(form, result);

            userManager.Verify();
            adminCourseService.Verify();
        }

        [Fact]
        public async Task CreatePost_ShouldRedirectToAction_GivenValidModelAndServiceSuccess()
        {
            // Arrange
            var form = this.GetCreateForm();

            var userManager = UserManagerMock.GetMock;
            userManager.FindByIdAsync(new User() { Id = form.TrainerId });

            var adminCourseService = new Mock<IAdminCourseService>();
            adminCourseService.CreateAsync(100);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            controller.TempData.AssertSuccessMsg(WebConstants.CourseCreatedMsg);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(redirectToActionResult);
            Assert.Equal(nameof(LearningSystem.Web.Controllers.CoursesController.Index), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);

            userManager.Verify();
            adminCourseService.Verify();
        }

        private void AssertEqualViewResultWithModel(CourseFormModel form, IActionResult result)
        {
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(CoursesController.CourseFormView, viewResult.ViewName);

            var model = Assert.IsType<CourseFormModel>(viewResult.Model);
            Assert.NotNull(model);

            this.AssertTrainersSelectList(model.Trainers);
            Assert.Equal(form, model);
        }

        private void AssertTrainersSelectList(IEnumerable<SelectListItem> resultTrainers)
        {
            var expectedTrainers = this.GetTrainers();

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

        private void AssertDate(DateTime resultDate)
        {
            var expectedDate = DateTime.Now;

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