namespace LearningSystem.Tests.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    public class UsersControllerTest
    {
        [Fact]
        public void UsersController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var attributes = typeof(UsersController).GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AuthorizeAttribute));

            attributes
                .Should()
                .Match(attr => attr.Any(a => a.GetType() == typeof(AuthorizeAttribute)));
        }

        [Fact]
        public void Certificate_ShouldAllowAnonymousUsers()
        {
            // Arrange
            var method = typeof(UsersController).GetMethod(nameof(UsersController.Certificate));

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AllowAnonymousAttribute));

            attributes
                .Should()
                .Match(attr => attr.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)));
        }

        [Fact]
        public void DownloadCertificate_ShouldAllowAnonymousUsers()
        {
            // Arrange
            var method = typeof(UsersController).GetMethod(nameof(UsersController.DownloadCertificate));

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AllowAnonymousAttribute));

            attributes
                .Should()
                .Match(attr => attr.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)));
        }

        [Fact]
        public async Task Profile_ShouldReturnRedirectToActionResult_WithInvalidUser()
        {
            var userManager = UserManagerMock.GetMock;
            userManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User)null);

            var controller = new UsersController(
                userManager.Object,
                userService: null, pdfService: null)
            {
                TempData = Tests.GetTempDataDictionary()
            };

            // Act
            var result = await controller.Profile();

            // Assert
            Assert.Contains(controller.TempData.Keys, k => k == WebConstants.TempDataErrorMessageKey);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToActionResult.ActionName);

            result
                .Should()
                .BeOfType<RedirectToActionResult>()
                .Subject
                .ActionName
                .Should()
                .BeEquivalentTo(nameof(HomeController.Index));
        }

        [Fact]
        public async Task Profile_ShouldReturnViewWithCorrectModel_WithValidUser()
        {
            // Arrange
            var userId = "MockId";

            var testRoles = new List<string>() { "testRole" };
            var testUser = new UserWithBirthdateServiceModel { Id = userId };
            var testCourses = new List<CourseProfileServiceModel>()
            {
                new CourseProfileServiceModel { Id = 1 },
                new CourseProfileServiceModel { Id = 2 },
            };

            var testViewModel = new UserProfileViewModel { User = testUser, Courses = testCourses, Roles = testRoles };

            var userManager = UserManagerMock.GetMock;
            userManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { Id = userId });
            userManager
                .Setup(u => u.GetRolesAsync(It.Is<User>(x => x.Id == userId)))
                .ReturnsAsync(testRoles);

            var userService = new Mock<IUserService>();
            userService
                .Setup(u => u.GetUserProfileAsync(It.Is<string>(id => id == userId)))
                .ReturnsAsync(new UserProfileServiceModel { User = testUser, Courses = testCourses });

            var controller = new UsersController(
                userManager.Object,
                userService.Object,
                pdfService: null);

            // Act
            var result = await controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);
            Assert.Equal(testViewModel.User, model.User);
            Assert.Equal(testViewModel.Courses, model.Courses);
            Assert.Equal(testViewModel.Roles, model.Roles);

            result
                .Should()
                .BeOfType<ViewResult>()
                .Subject
                .Model
                .Should()
                .BeOfType<UserProfileViewModel>()
                .And
                .Match(m => m.As<UserProfileViewModel>().User.Id == userId);
        }
    }
}
