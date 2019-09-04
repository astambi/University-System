namespace University.Tests.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using University.Tests.Mocks;
    using University.Web;
    using University.Web.Controllers;
    using Xunit;

    public class ExamsControllerTest
    {
        private const int TestExamId = 10;
        private const string TestUserId = "TestUserId";
        private const string FileUrl = "https://res.cloudinary.com/filename.pptx";

        [Fact]
        public void ExamsController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var authorizeAttribute = typeof(ExamsController)
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Create_ShouldHaveHpptPostAttribute()
        {
            // Act
            var method = typeof(ExamsController).GetMethod(nameof(ExamsController.Create));

            var httpPostAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(HttpPostAttribute))
                as HttpPostAttribute;

            // Assert
            Assert.NotNull(httpPostAttribute);
        }

        // More create tests to follow

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ExamsController(
                userManager.Object,
                cloudinaryService: null,
                courseService: null,
                examService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestExamId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenUserIsNotTrainerOrEnrolledStudent()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var examService = ExamServiceMock.GetMock;
            examService.CanBeDownloadedByUser(false);

            var controller = new ExamsController(
                userManager.Object,
                cloudinaryService: null,
                courseService: null,
                examService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestExamId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ExamDownloadUnauthorizedMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
                examService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenInvalidExam()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var examService = ExamServiceMock.GetMock;
            examService
                .CanBeDownloadedByUser(true)
                .GetDownloadUrlAsync(null);

            var controller = new ExamsController(
                userManager.Object,
                cloudinaryService: null,
                courseService: null,
                examService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestExamId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ExamNotFoundMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
                examService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToUrl_GivenValidInput()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var examService = ExamServiceMock.GetMock;
            examService
                .CanBeDownloadedByUser(true)
                .GetDownloadUrlAsync(FileUrl);

            var controller = new ExamsController(
                userManager.Object,
                cloudinaryService: null,
                courseService: null,
                examService.Object);

            using (controller)
            {
                // Act
                var result = await controller.Download(TestExamId);

                // Assert
                var redirectResult = Assert.IsType<RedirectResult>(result);
                Assert.Equal(FileUrl, redirectResult.Url);

                userManager.Verify();
                examService.Verify();
            }
        }

        private void AssertRedirectToCoursesIndex(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CoursesController.Index), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);
        }
    }
}
