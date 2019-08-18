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

        [Fact]
        public void UsersController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var attributes = typeof(UsersController).GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AuthorizeAttribute));
        }

        [Fact]
        public async Task Profile_ShouldReturnRedirectToActionResult_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserAsync(null);

            var controller = new UsersController(
                userManager.Object,
                userService: null)
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
                .GetProfileAsync(this.GetProfileUserData());

            var controller = new UsersController(
                userManager.Object,
                userService.Object);

            // Act
            var result = await controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);

            this.AssertProfile(model);

            userManager.Verify();
            userService.Verify();
        }

        private void AssertProfile(UserProfileViewModel model)
        {
            Assert.NotNull(model);

            this.AsserProfileUser(model.User);
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

        private void AsserProfileUser(UserProfileServiceModel profileUser)
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

        private IList<CourseProfileServiceModel> GetProfileCourses()
            => new List<CourseProfileServiceModel>()
            {
                new CourseProfileServiceModel { CourseId = 1, CourseName = "Name1", CourseStartDate = new DateTime(2019, 1, 15), CourseEndDate = new DateTime(2019, 4, 15), Grade = Grade.A, CertificateId = "1" },
                new CourseProfileServiceModel { CourseId = 2, CourseName = "Name2", CourseStartDate = new DateTime(2019, 3, 10), CourseEndDate = new DateTime(2019, 5, 10), Grade = null, CertificateId = null },
            };

        private UserProfileServiceModel GetProfileUserData()
            => new UserProfileServiceModel
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
