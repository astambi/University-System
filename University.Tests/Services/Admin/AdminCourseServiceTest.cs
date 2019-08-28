namespace University.Tests.Services.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin;
    using University.Services.Admin.Implementations;
    using University.Services.Admin.Models.Courses;
    using Xunit;

    public class AdminCourseServiceTest
    {
        private const int CourseIdInvalid = int.MinValue;
        private const int CourseValid = 10;

        private const string TrainerInvalid = "TrainerInvalid";
        private const string TrainerValid = "TrainerValid";

        [Fact]
        public async Task CreateAsync_ShouldNotSaveEntity_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUsers();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var resultInvalidTrainer = await adminCourseService.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<decimal>(),
                trainerId: TrainerInvalid);

            var resultInvalidName = await adminCourseService.CreateAsync(
                name: "        ",
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<decimal>(),
                TrainerValid);

            var resultInvalidDescription = await adminCourseService.CreateAsync(
                name: "Name",
                description: "        ",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<decimal>(),
                TrainerValid);

            var resultInvalidPrice = await adminCourseService.CreateAsync(
                name: "Name",
                description: "Description",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                -100,
                TrainerValid);

            // Assert
            Assert.Equal(CourseIdInvalid, resultInvalidTrainer);
            Assert.Equal(CourseIdInvalid, resultInvalidName);
            Assert.Equal(CourseIdInvalid, resultInvalidDescription);
            Assert.Equal(CourseIdInvalid, resultInvalidPrice);

            Assert.Empty(db.Courses);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUsers();
            var adminCourseService = this.InitializeAdminCourseService(db);

            var name = "    Name ";
            var description = "    Decription     ";
            var startDate = new DateTime(2019, 8, 15);
            var endDate = new DateTime(2019, 8, 15);
            var price = 150.00m;

            // Act
            var resultId = await adminCourseService.CreateAsync(
                name, description, startDate, endDate, price, TrainerValid);

            var course = db.Courses.Find(resultId);

            // Assert
            Assert.True(resultId > 0);
            Assert.NotNull(course);

            Assert.Equal(resultId, course.Id);
            Assert.Equal(name.Trim(), course.Name);
            Assert.Equal(description.Trim(), course.Description);
            Assert.Equal(price, course.Price);
            Assert.Equal(startDate.ToStartDateUtc(), course.StartDate);
            Assert.Equal(endDate.ToEndDateUtc(), course.EndDate);
        }

        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var result = await adminCourseService.RemoveAsync(CourseIdInvalid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteCorrectEntity_GivenValidCourse()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var result = await adminCourseService.RemoveAsync(CourseValid);
            var course = db.Courses.Find(CourseValid);

            // Assert
            Assert.True(result);
            Assert.Null(course);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var resultInvalidCourse = await adminCourseService.UpdateAsync(
                CourseIdInvalid,
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>(),
                TrainerValid);

            var resultInvalidTrainer = await adminCourseService.UpdateAsync(
                CourseValid,
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>(),
                TrainerInvalid);

            var resultInvalidName = await adminCourseService.UpdateAsync(
                CourseValid,
                name: "     ",
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>(),
                TrainerValid);

            var resultInvalidDescription = await adminCourseService.UpdateAsync(
                CourseValid, "Course name",
                description: "     ",
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), TrainerValid);

            var resultInvalidPrice = await adminCourseService.UpdateAsync(
                CourseValid, "Course name", "Description", It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                -150,
                TrainerValid);

            // Assert
            Assert.False(resultInvalidCourse);
            Assert.False(resultInvalidTrainer);
            Assert.False(resultInvalidName);
            Assert.False(resultInvalidDescription);
            Assert.False(resultInvalidPrice);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            var name = "  New Name   ";
            var description = "  New Decription     ";
            var startDate = new DateTime(2019, 8, 15);
            var endDate = new DateTime(2019, 8, 25);
            var price = 250.00m;

            // Act
            var result = await adminCourseService.UpdateAsync(
                CourseValid, name, description, startDate, endDate, price, TrainerValid);

            var course = db.Courses.Find(CourseValid);

            // Assert
            Assert.True(result);
            Assert.NotNull(course);

            Assert.Equal(CourseValid, course.Id);
            Assert.Equal(name.Trim(), course.Name);
            Assert.Equal(description.Trim(), course.Description);
            Assert.Equal(price, course.Price);
            Assert.Equal(startDate.ToStartDateUtc(), course.StartDate);
            Assert.Equal(endDate.ToEndDateUtc(), course.EndDate);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_GivenInvalidCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var result = await adminCourseService.GetByIdAsync(CourseIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_GivenValidCourse()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            var expected = db.Courses.Find(CourseValid);

            // Act
            var result = await adminCourseService.GetByIdAsync(CourseValid);

            // Assert
            Assert.IsType<AdminCourseServiceModel>(result);
            AssertCourse(expected, result);
        }

        [Fact]
        public async Task AllAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareCourses();
            var adminCourseService = this.InitializeAdminCourseService(db);

            // Act
            var result = await adminCourseService.AllAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AdminCourseBasicServiceModel>>(result);

            Assert.Equal(new[] { 3, 2, CourseValid }, result.Select(c => c.Id));

            foreach (var resultCourse in result)
            {
                var expectedCourse = db.Courses.Find(resultCourse.Id);
                AssertCourseBasic(expectedCourse, resultCourse);
            }
        }

        private static void AssertCourseBasic(Course expected, AdminCourseBasicServiceModel result)
        {
            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.StartDate, result.StartDate);
        }

        private static void AssertCourse(Course expected, AdminCourseServiceModel result)
        {
            Assert.Equal(CourseValid, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Description, result.Description);
            Assert.Equal(expected.StartDate, result.StartDate);
            Assert.Equal(expected.EndDate, result.EndDate);
            Assert.Equal(expected.Price, result.Price);
            Assert.Equal(expected.TrainerId, result.TrainerId);
        }

        private async Task<UniversityDbContext> PrepareCourses()
        {
            var course1 = new Course
            {
                Id = CourseValid,
                TrainerId = TrainerValid,
                Name = "Name BBB",
                Description = "Description 1",
                StartDate = new DateTime(2019, 8, 10),
                EndDate = new DateTime(2019, 8, 15),
                Price = 150
            };
            var course2 = new Course
            {
                Id = 2,
                TrainerId = TrainerValid,
                Name = "Name BBB",
                Description = "Description 2",
                StartDate = new DateTime(2019, 8, 15),
                EndDate = new DateTime(2019, 8, 17),
                Price = 250
            };
            var course3 = new Course
            {
                Id = 3,
                TrainerId = TrainerValid,
                Name = "Name AAA",
                Description = "Description 3",
                StartDate = new DateTime(2019, 8, 15),
                EndDate = new DateTime(2019, 8, 17),
                Price = 350
            };
            var trainer = new User { Id = TrainerValid };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3);
            await db.Users.AddAsync(trainer);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUsers()
        {
            var user1 = new User { Id = TrainerValid, Name = "Name 1", UserName = "UsernameBBBBB", Email = "email.1@gmail.com" };
            var user2 = new User { Id = "2", Name = "Name 2", UserName = "UsernameAAAAA", Email = "email.2@gmail.com" };
            var user3 = new User { Id = "3", Name = "Name 3", UserName = "UsernameCCCCC", Email = "email.3@gmail.com" };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(user1, user2, user3);
            await db.SaveChangesAsync();

            return db;
        }

        private IAdminCourseService InitializeAdminCourseService(UniversityDbContext db)
            => new AdminCourseService(db, Tests.Mapper);
    }
}
