namespace LearningSystem.Tests.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class UsersControllerTest
    {
        private const string TestId = "MockId";

        private const string TestScheme = "https";
        private const string TestHost = "mysite.com";
        private const string TestPath = "/certificate/" + TestId;
        private const string CertificateDloadUrl = TestScheme + "://" + TestHost + TestPath;

        [Fact]
        public void UsersController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var attributes = typeof(UsersController).GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AuthorizeAttribute));
        }

        [Fact]
        public void Certificate_ShouldAllowAnonymousUsers()
            => this.AssertAttributeAllowAnonymous(nameof(UsersController.Certificate));

        [Fact]
        public void Certificate_ShouldHaveRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(UsersController.Certificate));

        [Fact]
        public async Task Certificate_ShouldReturnRedirectToActionResult_GivenInvalidInput()
        {
            // Arrange
            var userService = UserServiceMock.GetMock;
            userService.GetCertificateDataAsync(null);

            var controller = new UsersController(
                userManager: null,
                userService.Object,
                pdfService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Certificate(TestId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            userService.Verify();
        }

        [Fact]
        public async Task Certificate_ShouldReturnViewResultWithCorrectModel_GivenValidInput()
        {
            // Arrange
            var userService = UserServiceMock.GetMock;
            userService.GetCertificateDataAsync(this.GetCertificate());

            var controller = new UsersController(
                userManager: null,
                userService.Object,
                pdfService: null)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(TestScheme, TestHost, TestPath); // HttpRequest Mock

            // Act
            var result = await controller.Certificate(TestId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateServiceModel>(viewResult.Model);

            this.AsserCertificate(model);

            userService.Verify();
        }

        [Fact]
        public void DownloadCertificate_ShouldAllowAnonymousUsers()
            => this.AssertAttributeAllowAnonymous(nameof(UsersController.DownloadCertificate));

        [Fact]
        public void DownloadCertificate_ShouldHaveRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(UsersController.DownloadCertificate));

        [Fact]
        public void DownloadCertificate_ShouldHaveHttpPostAttribute()
        {
            // Arrange
            var method = typeof(UsersController).GetMethod(nameof(UsersController.DownloadCertificate));

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(HttpPostAttribute));
        }

        [Fact]
        public void DownloadCertificate_ShouldReturnRedirectToActionResult_GivenInvalidPath()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(null);

            var controller = new UsersController(
                userManager: null, userService: null,
                pdfService.Object)
            {
                TempData = TempDataMock.GetMock,
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(TestScheme, TestHost, TestPath); // HttpRequest Mock

            // Act
            var result = controller.DownloadCertificate(TestId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            pdfService.Verify();
        }

        [Fact]
        public void DownloadCertificate_ShouldReturnFileContentResultWithCorrectContent_GivenServiceSuccess()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(this.GetCertificateFileBytes());

            var controller = new UsersController(
                userManager: null, userService: null,
                pdfService.Object)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(TestScheme, TestHost, TestPath); // HttpRequest Mock

            // Act
            var result = controller.DownloadCertificate(TestId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);

            this.AssertCertificateFileContent(fileContentResult);

            pdfService.Verify();
        }

        [Fact]
        public async Task Profile_ShouldReturnRedirectToActionResult_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserAsync(null);

            var controller = new UsersController(
                userManager.Object,
                userService: null, pdfService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Profile();

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            userManager.Verify();
        }

        [Fact]
        public async Task Profile_ShouldReturnViewResultWithCorrectModel_GivenValidUser()
        {
            // Arrange
            var testUser = new User() { Id = TestId };

            var userManager = UserManagerMock.GetMock;
            userManager
                .GetUserAsync(testUser)
                .GetRolesAsync(this.GetRoles());

            var userService = UserServiceMock.GetMock;
            userService
                .GetUserProfileCoursesAsync(this.GetProfileCourses())
                .GetUserProfileDataAsync(this.GetProfileUserData());

            var controller = new UsersController(
                userManager.Object,
                userService.Object,
                pdfService: null);

            // Act
            var result = await controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);

            this.AssertProfile(model);

            userManager.Verify();
            userService.Verify();
        }

        private void AssertAttributeAllowAnonymous(string methodName)
        {
            // Arrange
            var method = typeof(UsersController).GetMethod(methodName);

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AllowAnonymousAttribute));
        }

        private void AssertAttributeRouteWithId(string methodName)
        {
            // Arrange
            var method = typeof(UsersController).GetMethod(methodName);

            // Act
            var routeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            // Assert
            Assert.NotNull(routeAttribute);
            Assert.Equal(nameof(Certificate) + "/" + WebConstants.WithId, routeAttribute.Template);
        }

        private void AsserCertificate(CertificateServiceModel certificate)
        {
            var expectedCertificate = this.GetCertificate();

            Assert.NotNull(certificate);

            Assert.Equal(expectedCertificate.Id, certificate.Id);
            Assert.Equal(expectedCertificate.StudentName, certificate.StudentName);
            Assert.Equal(expectedCertificate.CourseName, certificate.CourseName);
            Assert.Equal(expectedCertificate.CourseStartDate, certificate.CourseStartDate);
            Assert.Equal(expectedCertificate.CourseEndDate, certificate.CourseEndDate);
            Assert.Equal(expectedCertificate.CourseTrainerName, certificate.CourseTrainerName);
            Assert.Equal(expectedCertificate.Grade, certificate.Grade);
            Assert.Equal(expectedCertificate.IssueDate, certificate.IssueDate);
            Assert.Equal(expectedCertificate.DownloadUrl, certificate.DownloadUrl);
        }

        private void AssertCertificateFileContent(FileContentResult fileContentResult)
        {
            Assert.Equal(this.GetCertificateFileBytes(), fileContentResult.FileContents);
            Assert.Equal(WebConstants.ApplicationPdf, fileContentResult.ContentType);
            Assert.Equal(WebConstants.CertificateFileName, fileContentResult.FileDownloadName);
        }

        private void AssertProfile(UserProfileViewModel model)
        {
            Assert.NotNull(model);

            this.AsserProfileUser(model.User);
            this.AsserProfileCourses(model.Courses);
            this.AsserProfileRoles(model.Roles);
        }

        private void AsserProfileCourses(IEnumerable<CourseProfileServiceModel> courses)
        {
            var expectedCourses = this.GetProfileCourses();

            Assert.NotNull(courses);
            Assert.Equal(expectedCourses.Count(), courses.Count());

            foreach (var expectedCourse in expectedCourses)
            {
                var actualCourse = courses.FirstOrDefault(c => c.CourseId == expectedCourse.CourseId);
                Assert.NotNull(actualCourse);

                Assert.Equal(expectedCourse.CourseName, actualCourse.CourseName);
                Assert.Equal(expectedCourse.CourseStartDate, actualCourse.CourseStartDate);
                Assert.Equal(expectedCourse.CourseEndDate, actualCourse.CourseEndDate);
                Assert.Equal(expectedCourse.CertificateId, actualCourse.CertificateId);
                Assert.Equal(expectedCourse.Grade, actualCourse.Grade);
            }
        }

        private void AsserProfileRoles(IEnumerable<string> roles)
        {
            var expectedRoles = this.GetRoles();

            Assert.NotNull(roles);
            Assert.Equal(expectedRoles.Count(), roles.Count());

            foreach (var expectedRole in expectedRoles)
            {
                var actualRole = roles.FirstOrDefault(r => r == expectedRole);

                Assert.NotNull(actualRole);
                Assert.Equal(expectedRole, actualRole);
            }
        }

        private void AsserProfileUser(UserWithBirthdateServiceModel profileUser)
        {
            var expectedUser = this.GetProfileUserData();

            Assert.NotNull(profileUser);

            Assert.Equal(expectedUser.Id, profileUser.Id);
            Assert.Equal(expectedUser.Name, profileUser.Name);
            Assert.Equal(expectedUser.Username, profileUser.Username);
            Assert.Equal(expectedUser.Email, profileUser.Email);
            Assert.Equal(expectedUser.Birthdate, profileUser.Birthdate);
        }

        private void AssertRedirectToHomeControllerIndex(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToActionResult.ActionName);
        }

        private CertificateServiceModel GetCertificate()
            => new CertificateServiceModel
            {
                Id = TestId,
                StudentName = "Student",
                CourseName = "Course",
                CourseStartDate = new DateTime(2019, 1, 1),
                CourseEndDate = new DateTime(2019, 5, 15),
                Grade = Grade.A,
                CourseTrainerName = "TrainerId",
                IssueDate = new DateTime(2019, 7, 10),
                DownloadUrl = CertificateDloadUrl
            };

        private byte[] GetCertificateFileBytes()
            => new byte[] { 101, 1, 27, 8, 11, 17, 57 };

        private IList<CourseProfileServiceModel> GetProfileCourses()
            => new List<CourseProfileServiceModel>()
            {
                new CourseProfileServiceModel { CourseId = 1, CourseName = "Name1", CourseStartDate = new DateTime(2019, 1, 15), CourseEndDate = new DateTime(2019, 4, 15), Grade = Grade.A, CertificateId = "1" },
                new CourseProfileServiceModel { CourseId = 2, CourseName = "Name2", CourseStartDate = new DateTime(2019, 3, 10), CourseEndDate = new DateTime(2019, 5, 10), Grade = null, CertificateId = null },
            };

        private UserWithBirthdateServiceModel GetProfileUserData()
            => new UserWithBirthdateServiceModel
            {
                Id = TestId,
                Name = "Name",
                Username = "Username",
                Birthdate = new DateTime(1990, 1, 1),
                Email = "myemail@email.com"
            };

        private IList<string> GetRoles()
            => new List<string>() { "StudentRole", "AdminRole" };
    }
}
