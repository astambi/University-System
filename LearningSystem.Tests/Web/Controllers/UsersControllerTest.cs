namespace LearningSystem.Tests.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
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
            string idInput = null;

            var userService = new Mock<IUserService>();
            userService
                .Setup(u => u.GetCertificateDataAsync(It.IsAny<string>()))
                .Callback((string idParam) => idInput = idParam) // service model input
                .ReturnsAsync((CertificateServiceModel)null) // invalid certificate
                .Verifiable();

            var controller = new UsersController(
                userManager: null,
                userService: userService.Object,
                pdfService: null)
            {
                TempData = Tests.GetTempDataDictionary()
            };

            // Act
            var result = await controller.Certificate(TestId);

            // Assert
            // Model Input
            Assert.NotNull(idInput);
            Assert.Equal(TestId, idInput); // correct model input

            // TempData
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataErrorMessageKey);
            Assert.Equal(
                WebConstants.CertificateNotFoundMsg,
                controller.TempData[WebConstants.TempDataErrorMessageKey]);

            // ActionResult
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(
                nameof(HomeController.Index),
                redirectToActionResult.ActionName);

            userService.Verify();
        }

        [Fact]
        public async Task Certificate_ShouldReturnViewResultWithCorrectModel_GivenValidInput()
        {
            // Arrange
            var userService = new Mock<IUserService>();
            userService
                .Setup(u => u.GetCertificateDataAsync(It.IsAny<string>()))
                .ReturnsAsync(this.GetCertificate())
                .Verifiable();

            var controller = new UsersController(
                userManager: null,
                userService: userService.Object,
                pdfService: null)
            {
                // Mock HttpRequest
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            // Mock HttpRequest
            this.SetupHttpRequestMock(controller.ControllerContext.HttpContext.Request);

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
            string urlInput = null;

            var pdfService = new Mock<IPdfService>();
            pdfService
                .Setup(s => s.ConvertToPdf(It.IsAny<string>()))
                .Callback((string urlParam) => urlInput = urlParam) // service model input
                .Returns((byte[])null) // invalid pdf
                .Verifiable();

            var controller = new UsersController(
                userManager: null, userService: null,
                pdfService: pdfService.Object)
            {
                TempData = Tests.GetTempDataDictionary(),
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } // Request mock
            };
            this.SetupHttpRequestMock(controller.ControllerContext.HttpContext.Request); // Request mock

            // Act
            var result = controller.DownloadCertificate(TestId);

            // Assert
            // Model Input
            Assert.NotNull(urlInput);
            Assert.Equal(CertificateDloadUrl, urlInput); // correct model input

            // TempData
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataErrorMessageKey);
            Assert.Equal(
                WebConstants.CertificateNotFoundMsg,
                controller.TempData[WebConstants.TempDataErrorMessageKey]);

            // ActionResult
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(
                nameof(HomeController.Index),
                redirectToActionResult.ActionName);

            pdfService.Verify();
        }

        [Fact]
        public void DownloadCertificate_ShouldReturnFileContentResultWithCorrectContent_GivenServiceSuccess()
        {
            // Arrange
            var pdfService = new Mock<IPdfService>();
            pdfService
                .Setup(s => s.ConvertToPdf(It.IsAny<string>()))
                .Returns(this.GetCertificateFileBytes())
                .Verifiable();

            var controller = new UsersController(
                userManager: null, userService: null,
                pdfService: pdfService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } // Request mock
            };
            this.SetupHttpRequestMock(controller.ControllerContext.HttpContext.Request); // Request mock

            // Act
            var result = controller.DownloadCertificate(TestId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(this.GetCertificateFileBytes(), fileContentResult.FileContents);
            Assert.Equal(WebConstants.ApplicationPdf, fileContentResult.ContentType);
            Assert.Equal(WebConstants.CertificateFileName, fileContentResult.FileDownloadName);

            pdfService.Verify();
        }

        [Fact]
        public async Task Profile_ShouldReturnRedirectToActionResult_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User)null) // invalid user
                .Verifiable();

            var controller = new UsersController(
                userManager.Object,
                userService: null, pdfService: null)
            {
                TempData = Tests.GetTempDataDictionary()
            };

            // Act
            var result = await controller.Profile();

            // Assert
            // TempData
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataErrorMessageKey);
            Assert.Equal(
                WebConstants.InvalidUserMsg,
                controller.TempData[WebConstants.TempDataErrorMessageKey]);

            // ActionResult
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToActionResult.ActionName);

            userManager.Verify();
        }

        [Fact]
        public async Task Profile_ShouldReturnViewResultWithCorrectModel_GivenValidUser()
        {
            // Arrange
            var testUser = new User() { Id = TestId };
            var testProfile = this.GetProfile();

            User userInput = null;
            string idInput = null;

            var userManager = UserManagerMock.GetMock;
            userManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser)
                .Verifiable();
            userManager
                .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .Callback((User userParam) => userInput = userParam) // service input
                .ReturnsAsync(this.GetRoles())
                .Verifiable();

            var userService = new Mock<IUserService>();
            userService
                .Setup(u => u.GetUserProfileAsync(It.IsAny<string>()))
                .Callback((string idParam) => idInput = idParam) // service input
                .ReturnsAsync(this.GetProfile())
                .Verifiable();

            var controller = new UsersController(
                userManager.Object,
                userService.Object,
                pdfService: null);

            // Act
            var result = await controller.Profile();

            // Assert
            // Model Input => UserService.GetUserProfileAsync
            Assert.NotNull(idInput);
            Assert.Equal(testUser.Id, idInput); // correct model input

            // Model Input => UserManager.GetRolesAsync
            Assert.NotNull(userInput);
            Assert.Equal(testUser.Id, userInput.Id); // correct model input

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);
            Assert.NotNull(model);

            this.AsserProfileUser(model.User);
            this.AsserProfileCourses(model.Courses);
            this.AsserProfileRoles(model.Roles);

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
            Assert.Equal(expectedCertificate.Student, certificate.Student);
            Assert.Equal(expectedCertificate.Course, certificate.Course);
            Assert.Equal(expectedCertificate.StartDate, certificate.StartDate);
            Assert.Equal(expectedCertificate.EndDate, certificate.EndDate);
            Assert.Equal(expectedCertificate.Trainer, certificate.Trainer);
            Assert.Equal(expectedCertificate.Grade, certificate.Grade);
            Assert.Equal(expectedCertificate.IssueDate, certificate.IssueDate);
            Assert.Equal(expectedCertificate.DownloadUrl, certificate.DownloadUrl);
        }

        private void AsserProfileCourses(IEnumerable<CourseProfileServiceModel> courses)
        {
            var expectedCourses = this.GetProfileCourses();

            Assert.NotNull(courses);
            Assert.Equal(expectedCourses.Count(), courses.Count());

            foreach (var expectedCourse in expectedCourses)
            {
                var actualCourse = courses.FirstOrDefault(c => c.Id == expectedCourse.Id);
                Assert.NotNull(actualCourse);

                Assert.Equal(expectedCourse.Name, actualCourse.Name);
                Assert.Equal(expectedCourse.StartDate, actualCourse.StartDate);
                Assert.Equal(expectedCourse.EndDate, actualCourse.EndDate);
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
            var expectedUser = this.GetProfileUser();

            Assert.NotNull(profileUser);

            Assert.Equal(expectedUser.Id, profileUser.Id);
            Assert.Equal(expectedUser.Name, profileUser.Name);
            Assert.Equal(expectedUser.Username, profileUser.Username);
            Assert.Equal(expectedUser.Email, profileUser.Email);
            Assert.Equal(expectedUser.Birthdate, profileUser.Birthdate);
        }

        private CertificateServiceModel GetCertificate()
            => new CertificateServiceModel
            {
                Id = TestId,
                Student = "Student",
                Course = "Course",
                StartDate = new DateTime(2019, 1, 1),
                EndDate = new DateTime(2019, 5, 15),
                Grade = Grade.A,
                Trainer = "TrainerId",
                IssueDate = new DateTime(2019, 7, 10),
                DownloadUrl = CertificateDloadUrl
            };

        private byte[] GetCertificateFileBytes()
            => new byte[] { 101, 1, 27, 8, 11, 17, 57 };

        private UserProfileServiceModel GetProfile()
            => new UserProfileServiceModel
            {
                User = this.GetProfileUser(),
                Courses = this.GetProfileCourses()
            };

        private IList<CourseProfileServiceModel> GetProfileCourses()
            => new List<CourseProfileServiceModel>()
            {
                new CourseProfileServiceModel { Id = 1, Name = "Name1", StartDate = new DateTime(2019, 1, 15), EndDate = new DateTime(2019, 4, 15), Grade = Grade.A, CertificateId = "1" },
                new CourseProfileServiceModel { Id = 2, Name = "Name2", StartDate = new DateTime(2019, 3, 10), EndDate = new DateTime(2019, 5, 10), Grade = null, CertificateId = null },
            };

        private UserWithBirthdateServiceModel GetProfileUser()
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

        private void SetupHttpRequestMock(HttpRequest req)
        {
            req.Scheme = TestScheme;
            req.Host = new HostString(TestHost);
            req.Path = new PathString(TestPath);
        }
    }
}
