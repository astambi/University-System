namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Courses;
    using University.Services.Models.ShoppingCart;
    using Xunit;

    public class CourseServiceTest
    {
        private const int CourseInvalid = -100;
        private const int CourseValid = 1;
        private const int CourseStarted = 20;
        private const int CourseStartedEnrolled = 22;
        private const int CourseStartedNotEnrolled = 25;
        private const int CourseFutureEnrolled = 30;
        private const int CourseFutureNotEnrolled = 33;

        private const int OrderValid = 1;
        private const int OrderInvalid = 2;
        private const int OrderWithoutItems = 10;
        private const int OrderWithCoursesEnrolled = 20;
        private const int OrderWithCoursesNotEnrolled = 25;
        private const int OrderWithCoursesStarted = 30;

        private const string StudentEnrolled = "Enrolled";
        private const string StudentInvalid = "Invalid";
        private const string StudentNotEnrolled = "NotEnrolled";
        private const string StudentValid = "Valid";

        private const int Precision = 20;

        [Fact]
        public async Task AllActive_ShouldReturnCorrectResult_BySearchFilterAndOrder()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            var expected = db.Courses
                .Where(c => c.Name.ToLower().Contains("t"))
                .Where(c => !c.EndDate.HasEnded())
                .OrderByDescending(c => c.StartDate)
                .OrderByDescending(c => c.EndDate)
                .ToList();

            // Act
            var result = await courseService.AllActiveAsync("T");
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(result);
            AssertCourseServiceModel(expected, resultList);
        }

        [Fact]
        public async Task AllActiveAsync_ShouldReturnCorrectResult_WithPagination()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultPage1Of2 = await courseService.AllActiveAsync("T", 1, 2);
            var resultPage2Of2 = await courseService.AllActiveAsync("T", 2, 2);

            var resultPagePositiveInvalid = await courseService.AllActiveAsync("T", 100, 12);

            var resultPageNegative = await courseService.AllActiveAsync("T", int.MinValue, 12);
            var resultPageSizeNegative = await courseService.AllActiveAsync("T", int.MinValue, int.MinValue);

            // Assert
            Assert.Equal(2, resultPage1Of2.Count());
            Assert.Equal(new List<int> { 3, 2 }, resultPage1Of2.Select(c => c.Id).ToList());

            Assert.Single(resultPage2Of2);
            Assert.Equal(new List<int> { 1 }, resultPage2Of2.Select(c => c.Id).ToList());

            Assert.Empty(resultPagePositiveInvalid);

            Assert.Equal(3, resultPageNegative.Count()); // default page = 1
            Assert.Equal(new List<int> { 3, 2, 1 }, resultPageNegative.Select(c => c.Id).ToList());

            Assert.Equal(3, resultPageSizeNegative.Count()); // default page = 1, pageSize = 12
            Assert.Equal(new List<int> { 3, 2, 1 }, resultPageSizeNegative.Select(c => c.Id).ToList());

            var resultList = resultPageNegative.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                AssertSingleCourse(db.Courses.Find(resultList[i].Id), resultList[i]);
            }
        }

        [Fact]
        public async Task AllArchivedAsync_ShouldReturnCorrectResult_BySearchFilterAndOrder()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            var expected = db.Courses
                .Where(c => c.Name.ToLower().Contains("t"))
                .Where(c => c.EndDate.HasEnded())
                .OrderByDescending(c => c.StartDate)
                .OrderByDescending(c => c.EndDate)
                .ToList();

            // Act
            var result = await courseService.AllArchivedAsync("T");

            // Assert
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(result);
            AssertCourseServiceModel(expected, resultList);
        }

        [Fact]
        public async Task AllArchivedAsync_ShouldReturnCorrectResult_WithPagination()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultPage1Of2 = await courseService.AllArchivedAsync("T", 1, 2);
            var resultPage2Of2 = await courseService.AllArchivedAsync("T", 2, 2);

            var resultPagePositiveInvalid = await courseService.AllArchivedAsync("T", 100, 12);

            var resultPageNegative = await courseService.AllArchivedAsync("T", int.MinValue, 12);
            var resultPageSizeNegative = await courseService.AllArchivedAsync("T", int.MinValue, int.MinValue);

            // Assert
            Assert.Equal(2, resultPage1Of2.Count());
            Assert.Equal(new List<int> { 7, 6 }, resultPage1Of2.Select(c => c.Id).ToList());

            Assert.Single(resultPage2Of2);
            Assert.Equal(new List<int> { 5 }, resultPage2Of2.Select(c => c.Id).ToList());

            Assert.Empty(resultPagePositiveInvalid);

            Assert.Equal(3, resultPageNegative.Count()); // default page = 1
            Assert.Equal(new List<int> { 7, 6, 5 }, resultPageNegative.Select(c => c.Id).ToList());

            Assert.Equal(3, resultPageSizeNegative.Count()); // default page = 1, pageSize = 12
            Assert.Equal(new List<int> { 7, 6, 5 }, resultPageSizeNegative.Select(c => c.Id).ToList());

            var resultList = resultPageNegative.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                AssertSingleCourse(db.Courses.Find(resultList[i].Id), resultList[i]);
            }
        }

        [Fact]
        public async Task TotalActiveAsync_ShouldReturnCorrectResult_BySearchFilter()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            var expected = db.Courses
                .Where(c => c.Name.ToLower().Contains("t"))
                .Where(c => !c.EndDate.HasEnded())
                .Count();

            // Act
            var result = await courseService.TotalActiveAsync("T");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task TotalArchivedAsync_ShouldReturnCorrectResult_BySearchFilter()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            var expected = db.Courses
                .Where(c => c.Name.ToLower().Contains("t"))
                .Where(c => c.EndDate.HasEnded())
                .Count();

            // Act
            var result = await courseService.TotalArchivedAsync("T");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CanEnrollAsync_ShouldReturnFalse_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.CanEnrollAsync(CourseInvalid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanEnrollAsync_ShouldReturnFalse_AfterCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultAfterStartDate = await courseService.CanEnrollAsync(CourseStartedNotEnrolled);
            var resultAfterStartDateEnrolled = await courseService.CanEnrollAsync(CourseStartedEnrolled);
            var resultOnStartDate = await courseService.CanEnrollAsync(CourseStarted);

            // Assert
            Assert.False(resultAfterStartDate);
            Assert.False(resultAfterStartDateEnrolled);
            Assert.False(resultOnStartDate);
        }

        [Fact]
        public async Task CanEnrollAsync_ShouldReturnTrue_BeforeCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultBeforeStartDate = await courseService.CanEnrollAsync(CourseFutureEnrolled);
            var resultBeforeStartDateEnrolled = await courseService.CanEnrollAsync(CourseFutureNotEnrolled);

            // Assert
            Assert.True(resultBeforeStartDate);
            Assert.True(resultBeforeStartDateEnrolled);
        }

        [Fact]
        public async Task EnrollStudentInCourseAsync_ShouldReturnFalse_GivenInvalidCourseOrUser()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result1 = await courseService.EnrollUserInCourseAsync(CourseInvalid, StudentNotEnrolled);
            var result2 = await courseService.EnrollUserInCourseAsync(CourseFutureNotEnrolled, null);
            var result3 = await courseService.EnrollUserInCourseAsync(CourseFutureNotEnrolled, StudentInvalid);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);

            Assert.Null(db.Find<StudentCourse>(StudentNotEnrolled, CourseInvalid));
            Assert.Null(db.Find<StudentCourse>(null, CourseFutureNotEnrolled));
            Assert.Null(db.Find<StudentCourse>(StudentInvalid, CourseFutureNotEnrolled));
        }

        [Fact]
        public async Task EnrollStudentInCourseAsync_ShouldNotAddStudentCourse_AfterCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result1 = await courseService.EnrollUserInCourseAsync(CourseStartedNotEnrolled, StudentNotEnrolled);
            var result2 = await courseService.EnrollUserInCourseAsync(CourseStartedEnrolled, StudentNotEnrolled);

            // Assert
            Assert.False(result1);
            Assert.False(result2);

            Assert.Null(db.Find<StudentCourse>(StudentNotEnrolled, CourseStartedNotEnrolled));
            Assert.Null(db.Find<StudentCourse>(StudentNotEnrolled, CourseStartedEnrolled));
        }

        [Fact]
        public async Task EnrollStudentInCourseAsync_ShouldAddValidStudentCourse_BeforeCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result1 = await courseService.EnrollUserInCourseAsync(CourseFutureNotEnrolled, StudentEnrolled);
            var result2 = await courseService.EnrollUserInCourseAsync(CourseFutureNotEnrolled, StudentNotEnrolled);
            var result3 = await courseService.EnrollUserInCourseAsync(CourseFutureEnrolled, StudentNotEnrolled);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);

            Assert.NotNull(db.Find<StudentCourse>(StudentEnrolled, CourseFutureNotEnrolled));
            Assert.NotNull(db.Find<StudentCourse>(StudentNotEnrolled, CourseFutureNotEnrolled));
            Assert.NotNull(db.Find<StudentCourse>(StudentNotEnrolled, CourseFutureEnrolled));
        }

        [Fact]
        public async Task CancellUserEnrollmentInCourseAsync_ShouldReturnFalse_GivenInvalidUserCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultInvalidCourse = await courseService.CancellUserEnrollmentInCourseAsync(CourseInvalid, StudentEnrolled);
            var resultInvalidUser = await courseService.CancellUserEnrollmentInCourseAsync(CourseFutureNotEnrolled, StudentNotEnrolled);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidCourse);
        }

        [Fact]
        public async Task CancellUserEnrollmentInCourseAsync_ShouldNotRemoveStudentCourse_AfterCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.CancellUserEnrollmentInCourseAsync(CourseStartedEnrolled, StudentEnrolled);
            var studentCourseAfterStartDate = db.Find<StudentCourse>(StudentEnrolled, CourseStartedEnrolled);

            // Assert
            Assert.False(result);
            Assert.NotNull(studentCourseAfterStartDate);
        }

        [Fact]
        public async Task CancellUserEnrollmentInCourseAsync_ShouldRemoveStudentCourse_BeforeCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.CancellUserEnrollmentInCourseAsync(CourseFutureEnrolled, StudentEnrolled);
            var studentCourseBeforeStartDate = db.Find<StudentCourse>(StudentEnrolled, CourseFutureEnrolled);

            // Assert
            Assert.True(result);
            Assert.Null(studentCourseBeforeStartDate);
        }

        [Fact]
        public async Task Exists_ShouldReturnFalse_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = courseService.Exists(CourseInvalid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Exists_ShouldReturnTrue_GivenValidCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = courseService.Exists(CourseFutureNotEnrolled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_GivenInvaliCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithDetails();
            var courseService = this.InitializeCourseService(db);

            // Act
            var invalidCourse = await courseService.GetByIdAsync(CourseInvalid);

            // Assert
            Assert.Null(invalidCourse);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_WithValidCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithDetails();
            var courseService = this.InitializeCourseService(db);

            const int CourseId = 3;
            var expectedCourse = db.Courses.Find(CourseId);

            // Act
            var validCourse = await courseService.GetByIdAsync(CourseId);

            // Assert
            Assert.NotNull(validCourse);
            Assert.IsType<CourseDetailsServiceModel>(validCourse);

            AssertSingleCourseDetails(expectedCourse, validCourse);
        }

        [Fact]
        public async Task GetByIdBasicAsync_ShouldReturnNull_GivenInvaliCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithDetails();
            var courseService = this.InitializeCourseService(db);

            // Act
            var invalidCourse = await courseService.GetByIdBasicAsync(CourseInvalid);

            // Assert
            Assert.Null(invalidCourse);
        }

        [Fact]
        public async Task GetByIdBasicAsync_ShouldReturnCorrectData_WithValidCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithDetails();
            var courseService = this.InitializeCourseService(db);

            const int CourseId = 3;
            var expectedCourse = db.Courses.Find(CourseId);

            // Act
            var validCourse = await courseService.GetByIdBasicAsync(CourseId);

            // Assert
            Assert.NotNull(validCourse);
            Assert.IsType<CourseServiceModel>(validCourse);

            AssertSingleCourse(expectedCourse, validCourse);
        }

        [Fact]
        public async Task IsUserEnrolledInCourseAsync_ShouldReturnFalse_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultInvalidCourse = await courseService.IsUserEnrolledInCourseAsync(CourseInvalid, StudentEnrolled);
            var resultInvalidStudent = await courseService.IsUserEnrolledInCourseAsync(CourseValid, StudentNotEnrolled);

            // Assert
            Assert.False(resultInvalidCourse);
            Assert.False(resultInvalidStudent);
        }

        [Fact]
        public async Task IsUserEnrolledInCourseAsync_ShouldReturnTrue_GivenCorrectInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.IsUserEnrolledInCourseAsync(CourseValid, StudentEnrolled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EnrollUserInOrderCoursesAsync_ShouldReturnFalse_GivenInvalidUserOrderOrNoOrderItems()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var resultInvalidUser = await courseService.EnrollUserInOrderCoursesAsync(OrderValid, StudentNotEnrolled);
            var resultInvalidOrder = await courseService.EnrollUserInOrderCoursesAsync(OrderInvalid, StudentEnrolled);
            var resultEmptyOrder = await courseService.EnrollUserInOrderCoursesAsync(OrderWithoutItems, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidOrder);
            Assert.False(resultEmptyOrder);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task EnrollUserInOrderCoursesAsync_ShouldReturnFalse_AfterCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.EnrollUserInOrderCoursesAsync(OrderWithCoursesStarted, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(result);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task EnrollUserInOrderCoursesAsync_ShouldReturnFalse_GivenUserIsEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.EnrollUserInOrderCoursesAsync(OrderWithCoursesEnrolled, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(result);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task EnrollUserInOrderCoursesAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.EnrollUserInOrderCoursesAsync(OrderWithCoursesNotEnrolled, StudentEnrolled);
            var studentCourse = db.Find<StudentCourse>(StudentEnrolled, CourseFutureNotEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.True(result);
            Assert.NotNull(studentCourse);
            Assert.Equal(1, countAfter - countBefore);
        }

        [Fact]
        public async Task CancelUserEnrollmentInOrderCoursesAsync_ShouldReturnFalse_GivenInvalidUserOrderOrNoOrderItems()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var resultInvalidUser = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderValid, StudentNotEnrolled);
            var resultInvalidOrder = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderInvalid, StudentEnrolled);
            var resultEmptyOrder = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderWithoutItems, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidOrder);
            Assert.False(resultEmptyOrder);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task CancelUserEnrollmentInOrderCoursesAsync_ShouldReturnFalse_AfterCourseStartDate()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderWithCoursesStarted, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(result);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task CancelUserEnrollmentInOrderCoursesAsync_ShouldReturnFalse_GivenUserIsNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);
            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderWithCoursesNotEnrolled, StudentEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.False(result);
            Assert.Equal(0, countAfter - countBefore);
        }

        [Fact]
        public async Task CancelUserEnrollmentInOrderCoursesAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareOrderWithCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            var countBefore = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Act
            var result = await courseService.CancelUserEnrollmentInOrderCoursesAsync(OrderWithCoursesEnrolled, StudentEnrolled);
            var studentCourseAfter = db.Find<StudentCourse>(StudentEnrolled, CourseFutureEnrolled);
            var countAfter = this.GetCoursesCountForUser(db, StudentEnrolled);

            // Assert
            Assert.True(result);
            Assert.Null(studentCourseAfter);

            Assert.Equal(-1, countAfter - countBefore);
        }

        [Fact]
        public async Task GetCartItemsDetailsForUserAsync_ShouldReturnCorrectData_GivenAnonymousUser()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            var cartItems = this.PrepareCartItemsWithCourses();

            // Act
            var result = await courseService.GetCartItemsDetailsForUserAsync(cartItems, null);
            var resulTCourseIds = result.Select(i => i.Id).ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItemDetailsServiceModel>>(result);

            AssertInvalidCartItems(resulTCourseIds);

            Assert.Contains(CourseFutureEnrolled, resulTCourseIds);
            Assert.Contains(CourseFutureNotEnrolled, resulTCourseIds);
        }

        [Fact]
        public async Task GetCartItemsDetailsForUserAsync_ShouldReturnCorrectData_GivenAuthUser()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            var cartItems = this.PrepareCartItemsWithCourses();

            // Act
            var result = await courseService.GetCartItemsDetailsForUserAsync(cartItems, StudentEnrolled);
            var resulTCourseIds = result.Select(i => i.Id).ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItemDetailsServiceModel>>(result);

            AssertInvalidCartItems(resulTCourseIds);
            Assert.DoesNotContain(CourseFutureEnrolled, resulTCourseIds);

            Assert.Contains(CourseFutureNotEnrolled, resulTCourseIds);

            var expected = db.Courses.Find(CourseFutureNotEnrolled);
            var resultItem = result.FirstOrDefault(i => i.Id == CourseFutureNotEnrolled);
            AssertCartItemDetails(expected, resultItem);
        }

        private static void AssertCartItemDetails(Course expected, CartItemDetailsServiceModel resultItem)
        {
            Assert.Equal(expected.Id, resultItem.Id);
            Assert.Equal(expected.Name, resultItem.Name);
            Assert.Equal(expected.Price, resultItem.Price);
            Assert.Equal(expected.StartDate, resultItem.StartDate);
        }

        private static void AssertInvalidCartItems(IEnumerable<int> resultIds)
        {
            Assert.DoesNotContain(CourseInvalid, resultIds);
            Assert.DoesNotContain(CourseStarted, resultIds);
            Assert.DoesNotContain(CourseStartedEnrolled, resultIds);
            Assert.DoesNotContain(CourseStartedNotEnrolled, resultIds);
        }

        private static void AssertCourseServiceModel(List<Course> expected, List<CourseServiceModel> result)
        {
            Assert.Equal(expected.Count, result.Count);

            for (var i = 0; i < result.Count; i++)
            {
                var expectedItem = expected[i];
                var resultItem = result[i];

                AssertSingleCourse(expectedItem, resultItem);
            }
        }

        private static void AssertSingleCourse(Course expectedItem, CourseServiceModel resultItem)
        {
            var dateTimeUtcNow = DateTime.UtcNow;

            Assert.Equal(expectedItem.Id, resultItem.Id);
            Assert.Equal(expectedItem.Name, resultItem.Name);
            Assert.Equal(expectedItem.StartDate, resultItem.StartDate);
            Assert.Equal(expectedItem.EndDate, resultItem.EndDate);
            Assert.Equal(expectedItem.Price, resultItem.Price);
            Assert.Equal(expectedItem.TrainerId, resultItem.TrainerId);
            Assert.Equal(expectedItem.Trainer.Name, resultItem.TrainerName);

            Assert.Equal(!expectedItem.StartDate.HasEnded(), resultItem.CanEnroll);
            Assert.Equal(expectedItem.StartDate.DaysTo(expectedItem.EndDate), resultItem.Duration);
            Assert.Equal(dateTimeUtcNow < expectedItem.StartDate, resultItem.IsUpcoming);
            Assert.Equal(
                expectedItem.StartDate <= dateTimeUtcNow && dateTimeUtcNow <= expectedItem.EndDate,
                resultItem.IsActive);

            resultItem.RemainingTimeTillStart
                .Should()
                .BeCloseTo(expectedItem.StartDate.RemainingTimeTillStart(), Precision);
        }

        private static void AssertSingleCourseDetails(Course expectedCourse, CourseDetailsServiceModel validCourse)
        {
            AssertSingleCourse(expectedCourse, validCourse);

            Assert.Equal(expectedCourse.Description, validCourse.Description);
            Assert.Equal(expectedCourse.Trainer.UserName, validCourse.TrainerUsername);
            Assert.Equal(expectedCourse.Students.Count(), validCourse.StudentsCount);
            Assert.Equal(expectedCourse.EndDate.IsToday(), validCourse.IsExamSubmissionDate);
        }

        private IEnumerable<CartItem> PrepareCartItemsWithCourses()
            => new List<CartItem>
            {
                new CartItem{ CourseId = CourseStartedNotEnrolled }, // past
                new CartItem{ CourseId = CourseStarted },  // past
                new CartItem{ CourseId = CourseFutureNotEnrolled },  // valid date
                new CartItem{ CourseId = CourseFutureEnrolled },  // valid date, enrolled
                new CartItem{ CourseId = CourseStartedEnrolled },  // past date, enrolled
                new CartItem{ CourseId = CourseInvalid },  // invalid
            };

        private async Task<UniversityDbContext> PrepareOrderWithCoursesToEnroll()
        {
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();

            var order1 = new Order { Id = OrderWithCoursesNotEnrolled, UserId = StudentEnrolled };
            var order2 = new Order { Id = OrderWithoutItems, UserId = StudentEnrolled }; // no items
            var order3 = new Order { Id = OrderWithCoursesEnrolled, UserId = StudentEnrolled };
            var order4 = new Order { Id = OrderWithCoursesStarted, UserId = StudentEnrolled };

            order1.OrderItems.Add(new OrderItem { CourseId = CourseFutureNotEnrolled }); // not enrolled
            order3.OrderItems.Add(new OrderItem { CourseId = CourseFutureEnrolled }); // enrolled
            order4.OrderItems.Add(new OrderItem { CourseId = CourseStartedNotEnrolled }); // course started

            var studentEnrolled = new User { Id = StudentEnrolled };
            var studentNotEnrolled = new User { Id = StudentNotEnrolled };

            var courses = new List<Course>
            {
                new Course{Id = CourseStartedNotEnrolled, StartDate = startDate.AddDays(-1) }, // past
                new Course{Id = CourseStarted, StartDate = startDate.AddDays(0) },  // past
                new Course{Id = CourseFutureNotEnrolled, StartDate = startDate.AddDays(1) },  // valid date
                new Course{Id = CourseFutureEnrolled, StartDate = startDate.AddDays(1) },  // valid date, enrolled
                new Course{Id = CourseStartedEnrolled, StartDate = startDate.AddDays(0) },  // past date, enrolled
            };

            studentEnrolled.Courses.Add(new StudentCourse { CourseId = CourseFutureEnrolled });
            studentEnrolled.Courses.Add(new StudentCourse { CourseId = CourseStartedEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(courses);
            await db.Users.AddRangeAsync(studentEnrolled, studentNotEnrolled);
            await db.Orders.AddRangeAsync(order1, order2, order3);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCoursesToEnroll()
        {
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();

            var studentEnrolled = new User { Id = StudentEnrolled };
            var studentNotEnrolled = new User { Id = StudentNotEnrolled };

            var courses = new List<Course>
            {
                new Course{Id = CourseStartedNotEnrolled, StartDate = startDate.AddDays(-1) }, // past
                new Course{Id = CourseStarted, StartDate = startDate.AddDays(0) },  // past
                new Course{Id = CourseFutureNotEnrolled, StartDate = startDate.AddDays(1), Name = "Future course not enrolled name", Price = 250 },  // valid date
                new Course{Id = CourseFutureEnrolled, StartDate = startDate.AddDays(1) },  // valid date, enrolled
                new Course{Id = CourseStartedEnrolled, StartDate = startDate.AddDays(0) },  // past date, enrolled
            };

            studentEnrolled.Courses.Add(new StudentCourse { CourseId = CourseFutureEnrolled });
            studentEnrolled.Courses.Add(new StudentCourse { CourseId = CourseStartedEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(courses);
            await db.Users.AddRangeAsync(studentEnrolled, studentNotEnrolled);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCoursesToSearch()
        {
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();
            var endDate = today.ToEndDateUtc();

            var trainer = new User { Id = StudentValid };
            var courses = new List<Course>
            {
                new Course{Id = 1, Name = "TTT", TrainerId = trainer.Id, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(0) },  // active 2
                new Course{Id = 2, Name = "ttt", TrainerId = trainer.Id, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(1) },  // active 1
                new Course{Id = 3, Name = "Tt",  TrainerId = trainer.Id, StartDate = startDate.AddDays(1),  EndDate = endDate.AddDays(1) },  // active 0
                new Course{Id = 4, Name = "XXX", TrainerId = trainer.Id, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(1) },  // no match
                new Course{Id = 5, Name = "TTT", TrainerId = trainer.Id, StartDate = startDate.AddDays(-2), EndDate = endDate.AddDays(-2) }, // archived 2
                new Course{Id = 6, Name = "ttt", TrainerId = trainer.Id, StartDate = startDate.AddDays(-2), EndDate = endDate.AddDays(-1) }, // archived 1
                new Course{Id = 7, Name = "Tt",  TrainerId = trainer.Id, StartDate = startDate.AddDays(-1), EndDate = endDate.AddDays(-1) }, // archived 0
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddAsync(trainer);
            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCoursesWithDetails()
        {
            //Users
            var users = new List<User>();
            for (var i = 1; i <= 5; i++)
            {
                var user = new User
                {
                    Id = i.ToString(),
                    Name = $"Name-{i}",
                    UserName = $"Username-{i}",
                    Email = $"Email-{i}@gmail.com"
                };

                users.Add(user);
            }

            // Courses
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();
            var endDate = today.ToEndDateUtc();
            var courses = new List<Course>();
            for (var i = 1; i <= 5; i++)
            {
                var course = new Course
                {
                    Id = i,
                    Name = $"Course-{i}",
                    StartDate = startDate.AddDays(i + 1),
                    EndDate = endDate.AddDays(i + 5)
                };

                courses.Add(course);
            }

            // StudentCourse
            var student = users.FirstOrDefault();
            for (var i = 0; i < courses.Count; i++)
            {
                var course = courses[i];
                course.TrainerId = student.Id;

                for (var j = 0; j <= i; j++)
                {
                    course.Students.Add(new StudentCourse { StudentId = users[j].Id });
                }
            }

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(users);
            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareStudentInCourse()
        {
            var student = new User { Id = StudentEnrolled };
            var course = new Course { Id = CourseValid };

            course.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddAsync(course);
            await db.Users.AddAsync(student);
            await db.SaveChangesAsync();

            return db;
        }

        private int GetCoursesCountForUser(UniversityDbContext db, string userId)
            => db
            .Users
            .Where(u => u.Id == userId)
            .Select(u => u.Courses.Count())
            .FirstOrDefault();

        private ICourseService InitializeCourseService(UniversityDbContext db)
            => new CourseService(db, Tests.Mapper);
    }
}
