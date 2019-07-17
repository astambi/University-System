﻿namespace LearningSystem.Tests.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using LearningSystem.Web.Models.Trainers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    public class TrainersControllerTest
    {
        private const int TestCourseId = 1;
        private const string TestSearchTerm = "TestSearchTerm";
        private const string TestStudentId = "TestStudentId";
        private const int TestTotalItems = 120;
        private const string TestUserId = "TestUserId";

        [Fact]
        public void TrainersController_ShouldBeAuthorizedForTrainerRoleOnly()
        {
            // Act
            var authorizeAttribute = typeof(TrainersController)
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(WebConstants.TrainerRole, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task Index_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager.Object,
                courseService: null, trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Index(It.IsAny<string>(), It.IsAny<int>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToCoursesControllerIndex(result);

            userManager.Verify();
        }

        [Theory]
        [InlineData(int.MinValue)] // invalid
        [InlineData(1)] // first
        [InlineData(5)]
        [InlineData(10)] // last
        [InlineData(int.MaxValue)] // invalid
        public async Task Index_ShouldReturnViewResultWithCorrectModel_GivenValidUser(int page)
        {
            // Arrange
            var testPagination = Tests.GetPaginationViewModel(page, TestTotalItems, TestSearchTerm);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.TotalCoursesAsync(TestTotalItems);
            trainerService.CoursesAsync(Tests.GetCourseServiceModelCollection());

            var controller = new TrainersController(
                userManager.Object,
                courseService: null,
                trainerService: trainerService.Object);

            // Act
            var result = await controller.Index(TestSearchTerm, page);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CoursePageListingViewModel>(viewResult.Model);

            this.AssertCoursePage(testPagination, model);

            userManager.Verify();
            trainerService.Verify();
        }

        private void AssertCoursePage(PaginationViewModel testPagination, CoursePageListingViewModel model)
        {
            Assert.NotNull(model);

            Tests.AssertCourseServiceModelCollection(model.Courses);
            Tests.AssertSearchViewModel(TestSearchTerm, model.Search);
            Tests.AssertPagination(testPagination, model.Pagination);
        }

        [Fact]
        public async Task Students_ShouldReturnRedirectToAction_GivenInvalidCourse()
        {
            // Arrange
            var courseService = new Mock<ICourseService>();
            courseService.Exists(false);

            var controller = new TrainersController(
                userManager: null,
                courseService: courseService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Students(TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task Students_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager: userManager.Object,
                courseService: courseService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Students(TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task Students_ShouldReturnRedirectToAction_GivenInvalidTrainer()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new TrainersController(
                userManager: userManager.Object,
                courseService: courseService.Object,
                trainerService: trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Students(TestCourseId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task Students_ShouldReturnViewResultWithCorrectModel_GivenValidInput()
        {
            // Arrange
            var testCourse = Tests.GetCourseServiceModelCollection().FirstOrDefault();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);
            trainerService.CourseByIdAsync(testCourse);
            trainerService.StudentsInCourseAsync(this.GetStudentInCourseServiceModelCollection());

            var controller = new TrainersController(
                userManager: userManager.Object,
                courseService: courseService.Object,
                trainerService: trainerService.Object);

            // Act
            var result = await controller.Students(TestCourseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudentCourseGradeViewModel>(viewResult.Model);

            Assert.NotNull(model);
            Tests.AssertCourseServiceModel(testCourse, model.Course);
            this.AssertStudentsInCourse(model.Students);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public void AssessPerformance_ShouldHaveHttpPostAttribute()
        {
            // Arrange
            var method = typeof(TrainersController).GetMethod(nameof(TrainersController.AssessPerformance));

            // Act 
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(HttpPostAttribute));
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToAction_GivenInvalidCourse()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new TrainersController(
                userManager: null,
                courseService: courseService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.AssessPerformance(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToActionWithCorrectRouteValues_GivenModelError()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var controller = new TrainersController(
                userManager: null,
                courseService: courseService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };
            controller.ModelState.AddModelError(string.Empty, "Error"); // model error

            // Act
            var result = await controller.AssessPerformance(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.StudentAssessmentErrorMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager.Object,
                courseService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.AssessPerformance(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            // Result
            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToAction_GivenInvalidTrainer()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new TrainersController(
                userManager.Object,
                courseService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.AssessPerformance(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToAction_GivenCourseHasNotEnded()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);
            trainerService.CourseHasEndedAsync(false);

            var controller = new TrainersController(
                userManager.Object,
                courseService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.AssessPerformance(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseHasNotEndedMsg);

            this.AssertRedirectToTrainersControllerStudents(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task AssessPerformance_ShouldReturnRedirectToActionWithCorrectRouteValues_GivenStudentNotEnrolledInCourses()
        {
            // Arrange
            var testModel = new StudentCourseGradeFormModel { CourseId = TestCourseId, StudentId = TestStudentId };

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);
            courseService.IsUserEnrolledInCourseAsync(false);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);
            trainerService.CourseHasEndedAsync(true);

            var controller = new TrainersController(
                userManager.Object,
                courseService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.AssessPerformance(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.StudentNotEnrolledInCourseMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        private RedirectToActionResult AssertRedirectToAction(IActionResult result)
            => Assert.IsType<RedirectToActionResult>(result);

        private void AssertRedirectToCoursesControllerIndex(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(CoursesController.Index), redirectToActionResult.ActionName);
        }

        private void AssertRedirectToTrainersControllerIndex(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(TrainersController.Index), redirectToActionResult.ActionName);
        }

        private void AssertRedirectToTrainersControllerStudents(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(TrainersController.Students), redirectToActionResult.ActionName);
        }

        private void AssertRouteWithId(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.Contains(redirectToActionResult.RouteValues.Keys, k => k == WebConstants.Id);
            Assert.Equal(TestCourseId, redirectToActionResult.RouteValues[WebConstants.Id]);
        }

        private void AssertStudentsInCourse(IEnumerable<StudentInCourseServiceModel> students)
        {
            var expectedStudents = this.GetStudentInCourseServiceModelCollection();

            Assert.NotNull(students);
            Assert.Equal(expectedStudents.Count(), students.Count());

            foreach (var expected in expectedStudents)
            {
                var actual = students.FirstOrDefault(st => st.Student.Id == expected.Student.Id);
                Assert.NotNull(actual);

                Assert.Equal(expected.Grade, actual.Grade);
                Assert.Equal(expected.HasExamSubmission, actual.HasExamSubmission);
                Assert.Equal(expected.Student.Name, actual.Student.Name);
                Assert.Equal(expected.Student.Username, actual.Student.Username);
                Assert.Equal(expected.Student.Email, actual.Student.Email);
            }
        }

        private IEnumerable<StudentInCourseServiceModel> GetStudentInCourseServiceModelCollection()
            => new List<StudentInCourseServiceModel>
            {
                new StudentInCourseServiceModel {  Student = new UserServiceModel { Id = "1", Name = "Name1", Username = "Username1", Email = "email.1@email.com" }, Grade = Grade.A, HasExamSubmission = true },
                new StudentInCourseServiceModel {  Student = new UserServiceModel { Id = "2", Name = "Name2", Username = "Username2", Email = "email.2@email.com" }, Grade = Grade.B, HasExamSubmission = true },
                new StudentInCourseServiceModel {  Student = new UserServiceModel { Id = "3", Name = "Name3", Username = "Username3", Email = "email.3@email.com" }, Grade = null, HasExamSubmission = true},
                new StudentInCourseServiceModel {  Student = new UserServiceModel { Id = "4", Name = "Name4", Username = "Username4", Email = "email.4@email.com" }, Grade = null, HasExamSubmission = false},
            };
    }
}
