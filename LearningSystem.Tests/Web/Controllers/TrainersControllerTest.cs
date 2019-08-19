﻿namespace LearningSystem.Tests.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Infrastructure.Helpers;
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

        // Attributes
        [Fact]
        public void TrainersController_ShouldHaveAuthorizeAttribute()
        {
            // Act
            var authorizeAttribute = typeof(TrainersController)
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Index_ShouldHaveAuthorizeAttribute_ForTrainerRole()
           => this.AssertAuthorizeAttributeForTrainerRole(nameof(TrainersController.Index));

        [Fact]
        public void DownloadExam_ShouldHaveAuthorizeAttribute_ForTrainerRole()
            => this.AssertAuthorizeAttributeForTrainerRole(nameof(TrainersController.DownloadExam));

        [Fact]
        public void EvaluateExam_ShouldHaveAuthorizeAttribute_ForTrainerRole()
           => this.AssertAuthorizeAttributeForTrainerRole(nameof(TrainersController.EvaluateExam));

        [Fact]
        public void EvaluateExam_ShouldHaveHttpPostAttribute()
        {
            // Arrange
            var method = typeof(TrainersController).GetMethod(nameof(TrainersController.EvaluateExam));

            // Act 
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(HttpPostAttribute));
        }

        [Fact]
        public void Resources_ShouldHaveAuthorizeAttribute_ForTrainerRole()
            => this.AssertAuthorizeAttributeForTrainerRole(nameof(TrainersController.Resources));

        [Fact]
        public void Students_ShouldHaveAuthorizeAttribute_ForTrainerRole()
            => this.AssertAuthorizeAttributeForTrainerRole(nameof(TrainersController.Students));

        // Methods
        [Fact]
        public async Task Index_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService: null,
                examService: null,
                trainerService: null)
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
            trainerService
                .TotalCoursesAsync(TestTotalItems)
                .CoursesAsync(Tests.GetCourseServiceModelCollection());

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService: null,
                examService: null,
                trainerService.Object);

            // Act
            var result = await controller.Index(TestSearchTerm, page);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CoursePageListingViewModel>(viewResult.Model);

            this.AssertCoursePage(testPagination, model);

            userManager.Verify();
            trainerService.Verify();
        }

        [Fact]
        public async Task Students_ShouldReturnRedirectToAction_GivenInvalidCourse()
        {
            // Arrange
            var courseService = new Mock<ICourseService>();
            courseService.Exists(false);

            var controller = new TrainersController(
                userManager: null,
                certificateService: null,
                courseService.Object,
                examService: null,
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
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
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
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
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
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseByIdAsync(testCourse)
                .StudentsInCourseAsync(this.GetStudentInCourseServiceModelCollection());

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object);

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
        public async Task EvaluateExam_ShouldReturnRedirectToAction_GivenInvalidCourse()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new TrainersController(
                userManager: null,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToActionWithCorrectRouteValues_GivenModelError()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var controller = new TrainersController(
                userManager: null,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };
            controller.ModelState.AddModelError(string.Empty, "Error"); // model error

            // Act
            var result = await controller.EvaluateExam(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.StudentAssessmentErrorMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            // Result
            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToAction_GivenInvalidTrainer()
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
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToAction_BeforeCourseEndDate()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(false);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, It.IsAny<StudentCourseGradeFormModel>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseHasNotEndedMsg);

            this.AssertRedirectToTrainersControllerStudents(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToActionWithCorrectRouteValues_GivenStudentNotEnrolledInCourses()
        {
            // Arrange
            var testModel = this.GetStudentInCourseWithGrade();

            var courseService = CourseServiceMock.GetMock;
            courseService
                .Exists(true)
                .IsUserEnrolledInCourseAsync(false);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.StudentNotEnrolledInCourseMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToActionWithCorrectRouteValues_GivenAssessmentError()
        {
            // Arrange
            var testModel = this.GetStudentInCourseWithGrade();

            var courseService = CourseServiceMock.GetMock;
            courseService
                .Exists(true)
                .IsUserEnrolledInCourseAsync(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var examService = ExamServiceMock.GetMock;
            examService.EvaluateAsync(false);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.ExamAssessmentErrorMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            courseService.Verify();
            examService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToActionWithCorrectRouteValuesAndAssessmentSuccessMsg_GivenAssessmentSuccessAndGradeNotEligibleForCertificate()
        {
            // Arrange
            var testModel = this.GetStudentInCourseWithGrade();

            var courseService = CourseServiceMock.GetMock;
            courseService
                .Exists(true)
                .IsUserEnrolledInCourseAsync(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var examService = ExamServiceMock.GetMock;
            examService.EvaluateAsync(true);

            var certificateService = CertificateServiceMock.GetMock;
            certificateService
                .IsGradeEligibleForCertificate(false);

            var controller = new TrainersController(
                userManager.Object,
                certificateService.Object,
                courseService.Object,
                examService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertSuccessMsg(WebConstants.ExamAssessedMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            certificateService.Verify();
            courseService.Verify();
            examService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task EvaluateExam_ShouldReturnRedirectToActionWithCorrectRouteValuesAndCertificateSuccessMsg_GivenCertificatetSuccess()
        {
            // Arrange
            var testModel = this.GetStudentInCourseWithGrade();

            var courseService = CourseServiceMock.GetMock;
            courseService
                .Exists(true)
                .IsUserEnrolledInCourseAsync(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var examService = ExamServiceMock.GetMock;
            examService.EvaluateAsync(true);

            var certificateService = CertificateServiceMock.GetMock;
            certificateService
                .IsGradeEligibleForCertificate(true)
                .CreateAsync(true);

            var controller = new TrainersController(
                userManager.Object,
                certificateService.Object,
                courseService.Object,
                examService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.EvaluateExam(TestCourseId, testModel);

            // Assert
            controller.TempData.AssertSuccessMsg(
                WebConstants.ExamAssessedMsg
                + Environment.NewLine
                + WebConstants.CertificateIssuedMsg);

            this.AssertRedirectToTrainersControllerStudents(result);
            this.AssertRouteWithId(result);

            certificateService.Verify();
            courseService.Verify();
            examService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnRedirectToAction_GivenInvalidCourse()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new TrainersController(
                userManager: null,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.DownloadExam(It.IsAny<int>(), It.IsAny<string>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnRedirectToAction_GivenInvalidUser()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.DownloadExam(It.IsAny<int>(), It.IsAny<string>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnRedirectToAction_GivenInvalidTrainer()
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
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.DownloadExam(It.IsAny<int>(), It.IsAny<string>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

            this.AssertRedirectToTrainersControllerIndex(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnRedirectToAction_BeforeCourseEndDate()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(false);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.DownloadExam(It.IsAny<int>(), It.IsAny<string>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CourseHasNotEndedMsg);

            this.AssertRedirectToTrainersControllerStudents(result);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnRedirectToAction_GivenInvalidExam()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var examService = ExamServiceMock.GetMock;
            examService.DownloadForTrainerAsync(null);

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.DownloadExam(It.IsAny<int>(), It.IsAny<string>());

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.StudentHasNotSubmittedExamMsg);

            this.AssertRedirectToTrainersControllerStudents(result);

            courseService.Verify();
            examService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        [Fact]
        public async Task DownloadExam_ShouldReturnFileContentResultWithCorrectContent_GivenDownloadSuccess()
        {
            // Arrange
            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService
                .IsTrainerForCourseAsync(true)
                .CourseHasEndedAsync(true);

            var examService = ExamServiceMock.GetMock;
            examService.DownloadForTrainerAsync(this.GetExamDownload());

            var controller = new TrainersController(
                userManager.Object,
                certificateService: null,
                courseService.Object,
                examService.Object,
                trainerService.Object);

            // Act
            var result = await controller.DownloadExam(TestCourseId, TestUserId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);

            this.AssertExamDownloadFile(fileContentResult);

            courseService.Verify();
            trainerService.Verify();
            userManager.Verify();
        }

        private void AssertAuthorizeAttribute(string methodName)
        {
            // Arrange
            var method = typeof(TrainersController).GetMethod(methodName);

            // Act 
            var authorizeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        private void AssertAuthorizeAttributeForTrainerRole(string methodName)
        {
            // Arrange
            var method = typeof(TrainersController).GetMethod(methodName);

            // Act 
            var authorizeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(WebConstants.TrainerRole, authorizeAttribute.Roles);
        }

        private void AssertExamDownloadFile(FileContentResult fileContentResult)
        {
            var expectedExam = this.GetExamDownload();
            var expectedFileName = FileHelpers.ExamFileName(expectedExam.CourseName, expectedExam.StudentUserName, expectedExam.SubmissionDate);

            Assert.NotNull(fileContentResult);

            Assert.Equal(expectedExam.FileSubmission, fileContentResult.FileContents);
            Assert.Equal(WebConstants.ApplicationZip, fileContentResult.ContentType);
            Assert.Equal(expectedFileName, fileContentResult.FileDownloadName);
        }

        private void AssertCoursePage(PaginationViewModel testPagination, CoursePageListingViewModel model)
        {
            Assert.NotNull(model);

            Tests.AssertCourseServiceModelCollection(model.Courses);
            Tests.AssertSearchViewModel(TestSearchTerm, model.Search);
            Tests.AssertPagination(testPagination, model.Pagination);
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
                var actual = students.FirstOrDefault(st => st.StudentId == expected.StudentId);
                Assert.NotNull(actual);

                Assert.Equal(expected.Grade, actual.Grade);
                Assert.Equal(expected.HasExamSubmission, actual.HasExamSubmission);
                Assert.Equal(expected.StudentName, actual.StudentName);
                Assert.Equal(expected.StudentUserName, actual.StudentUserName);
                Assert.Equal(expected.StudentEmail, actual.StudentEmail);
            }
        }

        private ExamDownloadServiceModel GetExamDownload()
            => new ExamDownloadServiceModel
            {
                FileSubmission = new byte[] { 111, 17, 11, 37, 55, 23, 8 },
                SubmissionDate = new DateTime(2019, 7, 18),
                StudentUserName = "TestStudent",
                CourseName = "TestCourse"
            };

        private IEnumerable<StudentInCourseServiceModel> GetStudentInCourseServiceModelCollection()
            => new List<StudentInCourseServiceModel>
            {
                new StudentInCourseServiceModel { StudentId = "1", StudentName = "Name1", StudentUserName = "Username1", StudentEmail = "email.1@email.com", Grade = Grade.A, HasExamSubmission = true },
                new StudentInCourseServiceModel { StudentId = "2", StudentName = "Name2", StudentUserName = "Username2", StudentEmail = "email.2@email.com", Grade = Grade.B, HasExamSubmission = true },
                new StudentInCourseServiceModel { StudentId = "3", StudentName = "Name3", StudentUserName = "Username3", StudentEmail = "email.3@email.com", Grade = null, HasExamSubmission = true},
                new StudentInCourseServiceModel { StudentId = "4", StudentName = "Name4", StudentUserName = "Username4", StudentEmail = "email.4@email.com", Grade = null, HasExamSubmission = false},
            };

        private StudentCourseGradeFormModel GetStudentInCourseWithGrade()
            => new StudentCourseGradeFormModel { CourseId = TestCourseId, StudentId = TestStudentId, Grade = Grade.A };
    }
}
