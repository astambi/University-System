namespace LearningSystem.Tests.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Web;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Infrastructure.Helpers;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    public class HomeControllerTest
    {
        private const string TestSearchTerm = "testSearch";
        private const int TestTotalItems = 120;

        private int testRequestedPage;
        private int testTotalPages;
        private int testCurrentPage;
        private int testPreviousPage;
        private int testNextPage;

        [Fact]
        public void HomeController_ShouldBeForAllUsers()
        {
            // Act
            var attributes = typeof(HomeController).GetCustomAttributes(true);

            // Assert
            Assert.DoesNotContain(attributes, a => a.GetType() == typeof(AuthorizeAttribute));
        }

        [Theory]
        [InlineData(int.MinValue)] // invalid
        [InlineData(1)] // first
        [InlineData(5)]
        [InlineData(10)] // last
        [InlineData(int.MaxValue)] // invalid
        public async Task HomeController_ShouldReturnViewResultWithCorrectModel(int requestedPage)
        {
            // Arrange
            this.GetPagination(requestedPage);

            string searchInputTotal = null;
            string searchInputAll = null;
            var pageInput = int.MinValue;
            var pageSizeInput = int.MinValue;

            var courseService = new Mock<ICourseService>();
            courseService
                .Setup(s => s.TotalActiveAsync(It.IsAny<string>()))
                .Callback((string searchParam) => searchInputTotal = searchParam)
                .ReturnsAsync(TestTotalItems)
                .Verifiable();
            courseService
                .Setup(s => s.AllActiveWithTrainersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback((string searchParam, int pageParam, int pageSizeParam) =>
                {
                    searchInputAll = searchParam;
                    pageInput = pageParam;
                    pageSizeInput = pageSizeParam;
                })
                .ReturnsAsync(this.GetCourses())
                .Verifiable();

            var controller = new HomeController(courseService.Object);

            // Act
            var result = await controller.Index(TestSearchTerm, this.testRequestedPage);

            // Assert
            // Service Input Model => TotalActiveAsync
            Assert.Equal(TestSearchTerm, searchInputTotal);

            // Service Input Model => AllActiveWithTrainersAsync
            Assert.Equal(TestSearchTerm, searchInputAll);
            Assert.Equal(this.testCurrentPage, pageInput);
            Assert.Equal(WebConstants.PageSize, pageSizeInput);

            // View result
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CoursePageListingViewModel>(viewResult.Model);

            Assert.NotNull(model);
            this.AssertCourses(model.Courses);
            this.AssertSearch(model.Search);
            this.AssertPagination(model.Pagination);

            courseService.Verify();
        }

        private void AssertCourses(IEnumerable<CourseServiceModel> courses)
        {
            var expectedCourses = this.GetCourses();

            Assert.NotNull(courses);
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(courses);

            Assert.Equal(expectedCourses.Count(), courses.Count());

            foreach (var expectedCourse in expectedCourses)
            {
                var actualCourse = courses.FirstOrDefault(c => c.Id == expectedCourse.Id);
                Assert.NotNull(actualCourse);

                Assert.Equal(expectedCourse.Name, actualCourse.Name);
                Assert.Equal(expectedCourse.StartDate, actualCourse.StartDate);
                Assert.Equal(expectedCourse.EndDate, actualCourse.EndDate);
                Assert.Equal(expectedCourse.TrainerId, actualCourse.TrainerId);
                Assert.Equal(expectedCourse.TrainerName, actualCourse.TrainerName);
            }
        }

        private void AssertPagination(PaginationViewModel pagination)
        {
            Assert.NotNull(pagination);
            Assert.IsType<PaginationViewModel>(pagination);

            Assert.Equal(nameof(HomeController.Index), pagination.Action);
            Assert.Equal(TestSearchTerm, pagination.SearchTerm);
            Assert.Equal(TestTotalItems, pagination.TotalItems);
            Assert.Equal(this.testRequestedPage, pagination.RequestedPage);
            Assert.Equal(this.testTotalPages, pagination.TotalPages);
            Assert.Equal(this.testCurrentPage, pagination.CurrentPage);
            Assert.Equal(this.testPreviousPage, pagination.PreviousPage);
            Assert.Equal(this.testNextPage, pagination.NextPage);
        }

        private void AssertSearch(SearchViewModel search)
        {
            Assert.NotNull(search);
            Assert.IsType<SearchViewModel>(search);

            Assert.Equal(TestSearchTerm, search.SearchTerm);
            Assert.Equal(FormActionEnum.Search, search.Action);
            Assert.Equal(WebConstants.SearchByCourseName, search.Placeholder);
        }

        private IEnumerable<CourseServiceModel> GetCourses()
            => new List<CourseServiceModel>()
            {
                new CourseServiceModel{ Id = 1, Name ="Name1", StartDate = new DateTime(2019, 1, 12), EndDate = new DateTime(2019, 3, 11), TrainerId = "1", TrainerName="Trainer1" },
                new CourseServiceModel{ Id = 2, Name ="Name1", StartDate = new DateTime(2019, 3, 10), EndDate = new DateTime(2019, 4, 13), TrainerId = "2", TrainerName="Trainer2" },
                new CourseServiceModel{ Id = 3, Name ="Name1", StartDate = new DateTime(2019, 5, 11), EndDate = new DateTime(2019, 5, 17), TrainerId = "1", TrainerName="Trainer1" },
            };

        private void GetPagination(int requestedPage = 1)
        {
            this.testRequestedPage = requestedPage;
            this.testTotalPages = PaginationHelpers.GetTotalPages(TestTotalItems, WebConstants.PageSize);
            this.testCurrentPage = PaginationHelpers.GetValidCurrentPage(this.testRequestedPage, this.testTotalPages);
            this.testPreviousPage = this.testCurrentPage == 1 ? 1 : this.testCurrentPage - 1;
            this.testNextPage = this.testCurrentPage == this.testTotalPages ? this.testTotalPages : this.testCurrentPage + 1;
        }
    }
}
