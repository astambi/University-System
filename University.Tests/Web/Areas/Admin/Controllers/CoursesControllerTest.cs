namespace University.Tests.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Data.Models;
    using University.Services.Admin.Models.Courses;
    using University.Tests.Mocks;
    using University.Web;
    using University.Web.Areas.Admin.Controllers;
    using University.Web.Areas.Admin.Models.Courses;
    using University.Web.Models;
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

        private const int TestCourseId = 100;

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

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(null);

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

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(new User() { Id = form.TrainerId });

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

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(new User() { Id = form.TrainerId });

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
            controller.TempData.AssertErrorMsg(WebConstants.CourseCreateErrorMsg);

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

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.CreateAsync(TestCourseId);

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
            controller.TempData.AssertSuccessMsg(WebConstants.CourseCreateSuccessMsg);

            this.AssertRedirectToCoursesIndex(result);

            userManager.Verify();
            adminCourseService.Verify();
        }

        [Fact]
        public async Task EditGet_ShouldReturnRedirectToActionWithCorrectData_GivenInvalidCourse()
        {
            // Arrange
            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.GetByIdAsync(null);

            var controller = new CoursesController(
                userManager: null,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Edit(TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToCoursesIndex(result);

            adminCourseService.Verify();
        }

        [Fact]
        public async Task EditGet_ShouldReturnViewResultWithCorrectModel_GivenValidCourse()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(this.GetTrainers());

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.GetByIdAsync(this.GetAdminCourse());

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null,
                mapper: Tests.Mapper);

            // Act
            var result = await controller.Edit(TestCourseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(CoursesController.CourseFormView, viewResult.ViewName);

            var model = Assert.IsType<CourseFormModel>(viewResult.Model);
            this.AssertAdminCourseForm(model, FormActionEnum.Edit);

            userManager.Verify();
            adminCourseService.Verify();
        }

        [Fact]
        public async Task EditPost_ShouldReturnRedirectToActionWithCorrectData_GivenInvalidCourse()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new CoursesController(
                userManager: null,
                adminCourseService: null,
                courseService.Object,
                mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Edit(TestCourseId, this.GetEditDeleteForm());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToCoursesIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task EditPost_ShouldReturnViewWithCorrectModel_GivenInvalidTrainer()
        {
            // Arrange
            var form = this.GetEditDeleteForm();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(null);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null,
                courseService.Object,
                mapper: null);

            // Act
            var result = await controller.Edit(TestCourseId, form);

            // Assert
            this.AssertEqualViewResultWithModel(form, result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EditPost_ShouldReturnViewWithCorrectModel_GivenInvalidModelState()
        {
            // Arrange
            var form = this.GetEditDeleteForm();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(new User { Id = form.TrainerId });

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService: null,
                courseService.Object,
                mapper: null);

            controller.ModelState.AddModelError(string.Empty, "Error"); // model error

            // Act
            var result = await controller.Edit(TestCourseId, form);

            // Assert
            this.AssertEqualViewResultWithModel(form, result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EditPost_ShouldReturnViewWithCorrectModel_GivenServiceError()
        {
            // Arrange
            var form = this.GetEditDeleteForm();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUsersInRoleTrainerAsync(this.GetTrainers())
                .FindByIdAsync(new User { Id = form.TrainerId });

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.UpdateAsync(false);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService.Object,
                mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Edit(TestCourseId, form);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseUpdateErrorMsg);

            this.AssertEqualViewResultWithModel(form, result);

            adminCourseService.Verify();
            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EditPost_ShouldReturnRedirectWithCorrectRoute_GivenUpdateSuccess()
        {
            // Arrange
            var form = this.GetEditDeleteForm();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.FindByIdAsync(new User { Id = form.TrainerId });

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.UpdateAsync(true);

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService.Object,
                mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Edit(TestCourseId, form);

            // Assert
            controller.TempData.AssertSuccessMsg(WebConstants.CourseUpdateSuccessMsg);

            this.AssertRedirectToCoursesDetailsWithCorrectRouteValues(result);

            adminCourseService.Verify();
            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DeleteGet_ShouldReturnRedirectToActionWithCorrectData_GivenInvalidCourse()
        {
            // Arrange
            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.GetByIdAsync(null);

            var controller = new CoursesController(
                userManager: null,
                adminCourseService.Object,
                courseService: null, mapper: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Delete(TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToCoursesIndex(result);

            adminCourseService.Verify();
        }

        [Fact]
        public async Task DeleteGet_ShouldReturnViewResultWithCorrectModel_GivenValidCourse()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUsersInRoleTrainerAsync(this.GetTrainers());

            var adminCourseService = AdminCourseServiceMock.GetMock;
            adminCourseService.GetByIdAsync(this.GetAdminCourse());

            var controller = new CoursesController(
                userManager.Object,
                adminCourseService.Object,
                courseService: null,
                mapper: Tests.Mapper);

            // Act
            var result = await controller.Delete(TestCourseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(CoursesController.CourseFormView, viewResult.ViewName);

            var model = Assert.IsType<CourseFormModel>(viewResult.Model);
            this.AssertAdminCourseForm(model, FormActionEnum.Delete);

            userManager.Verify();
            adminCourseService.Verify();
        }

        private void AssertAdminCourseForm(CourseFormModel model, FormActionEnum expectedAction)
        {
            var expectedCourse = this.GetAdminCourse();

            Assert.NotNull(model);
            Assert.Equal(expectedAction, model.Action);
            Assert.Equal(expectedCourse.Name, model.Name);
            Assert.Equal(expectedCourse.Description, model.Description);
            Assert.Equal(expectedCourse.StartDate, model.StartDate);
            Assert.Equal(expectedCourse.EndDate, model.EndDate);
            Assert.Equal(expectedCourse.TrainerId, model.TrainerId);

            this.AssertTrainersSelectList(model.Trainers);
        }

        private void AssertDate(DateTime resultDate)
        {
            var expectedDate = DateTime.Now;

            Assert.Equal(expectedDate.Year, resultDate.Year);
            Assert.Equal(expectedDate.Month, resultDate.Month);
            Assert.Equal(expectedDate.Day, resultDate.Day);
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

        private void AssertRedirectToCoursesDetailsWithCorrectRouteValues(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(redirectToActionResult);

            Assert.Equal(nameof(University.Web.Controllers.CoursesController.Details), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);

            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.Contains(redirectToActionResult.RouteValues.Keys, k => k == WebConstants.Id);
            Assert.Equal(TestCourseId, redirectToActionResult.RouteValues[WebConstants.Id]);
        }

        private void AssertRedirectToCoursesIndex(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(redirectToActionResult);

            Assert.Equal(nameof(University.Web.Controllers.CoursesController.Index), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);
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

        private AdminCourseServiceModel GetAdminCourse()
            => new AdminCourseServiceModel
            {
                Id = TestCourseId,
                Name = "Name",
                Description = "Description",
                StartDate = new DateTime(2019, 3, 10).ToLocalTime(),
                EndDate = new DateTime(2019, 6, 30).ToLocalTime(),
                TrainerId = "TrainerId"
            };

        private CourseFormModel GetCreateForm()
            => new CourseFormModel()
            {
                Name = "Name",
                Description = "Description",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(15),
                TrainerId = "TrainerId"
            };

        private CourseFormModel GetEditDeleteForm()
            => new CourseFormModel()
            {
                Name = "Name",
                Description = "Description",
                StartDate = new DateTime(2019, 3, 10).ToLocalTime(),
                EndDate = new DateTime(2019, 6, 30).ToLocalTime(),
                TrainerId = "TrainerId"
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