namespace LearningSystem.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Moq;
    using Xunit;

    public class UserServiceTest
    {
        private const string UserIdValid = "UserValid";
        private const string UserIdInvalid = "UserInvalid";
        private const string UserEmail = "user@gmail.com";
        private const string UserName = "User Name";
        private const string UserNameEdit = "     User Name     ";
        private const string UserUsername = "Username";

        private readonly DateTime UserBirthDate = new DateTime(1990, 3, 15);
        private readonly DateTime UserBirthDateToEdit = new DateTime(1990, 3, 25);

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenUserHasArticles()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await db.Articles.AddAsync(new Article { AuthorId = UserIdValid });
            await db.SaveChangesAsync();

            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenUserIsCourseTrainer()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await db.Courses.AddAsync(new Course { TrainerId = UserIdValid });
            await db.SaveChangesAsync();

            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnTrue_GivenUserIsNotCourseTrainerAndHasNoArticle()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetProfileToEditAsync_ShouldReturnNull_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileToEditAsync(UserIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileToEditAsync_ShouldReturnCorrectData_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileToEditAsync(UserIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserEditServiceModel>(result);

            Assert.Equal(UserName, result.Name);
            Assert.Equal(this.UserBirthDate, result.Birthdate);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturCorrectData_GivenValidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileAsync(UserIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnNull_GivenInvalidUser()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileAsync(UserIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserProfileServiceModel>(result);

            Assert.Equal(UserIdValid, result.Id);
            Assert.Equal(UserEmail, result.Email);
            Assert.Equal(UserUsername, result.Username);
            Assert.Equal(UserName, result.Name);
            Assert.Equal(this.UserBirthDate, result.Birthdate);
        }

        [Fact]
        public async Task GetCoursesAsync_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetCoursesAsync(UserIdInvalid);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCoursesAsync_ShouldReturnCorrectData_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserCourses();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetCoursesAsync(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseProfileServiceModel>>(result);
            Assert.NotEmpty(result);

            var resultList = result.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                var resultItem = resultList[i];
                var expected = db.Courses
                    .Where(c => c.Id == resultItem.CourseId)
                    .Select(c => new
                    {
                        Course = c,
                        Grade = c.Students
                            .Where(sc => sc.StudentId == UserIdValid)
                            .Select(sc => sc.Grade)
                            .FirstOrDefault(),
                        CertificateId = c.Certificates
                            .Where(cert => cert.StudentId == UserIdValid)
                            .Select(cert => cert.Id)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                var expectedCourse = expected.Course;
                Assert.Equal(expectedCourse.Id, resultItem.CourseId);
                Assert.Equal(expectedCourse.Name, resultItem.CourseName);
                Assert.Equal(expectedCourse.StartDate, resultItem.CourseStartDate);
                Assert.Equal(expectedCourse.EndDate, resultItem.CourseEndDate);

                Assert.Equal(expected.Grade, resultItem.Grade);
                Assert.Equal(expected.CertificateId, resultItem.CertificateId);
            }

            // Assert Collection Sorting
            var resultIdsSorting = result.Select(c => c.CourseId).ToList();
            var expectedIdsSorting = db.Courses
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .Select(c => c.Id)
                .ToList();

            Assert.Equal(expectedIdsSorting, resultIdsSorting);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldNotUpdate_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var resultInvalidUser = await userService.UpdateProfileAsync(
                UserIdInvalid,
                It.IsAny<string>(),
                DateTime.Now);

            var resultInvalidName = await userService.UpdateProfileAsync(
                UserIdValid,
                name: "          ",
                DateTime.Now);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidName);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.UpdateProfileAsync(
                UserIdValid, UserNameEdit, this.UserBirthDateToEdit);

            var resultUser = await db.Users.FindAsync(UserIdValid);

            // Assert
            Assert.True(result);
            Assert.Equal(UserNameEdit.Trim(), resultUser.Name);
            Assert.Equal(this.UserBirthDateToEdit, resultUser.Birthdate);
        }

        private async Task<LearningSystemDbContext> PrepareUser()
        {
            var db = Tests.InitializeDatabase();
            await db.Users.AddAsync(new User
            {
                Id = UserIdValid,
                UserName = UserUsername,
                Email = UserEmail,
                Name = UserName,
                Birthdate = UserBirthDate
            });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<LearningSystemDbContext> PrepareUserCourses()
        {
            var course1 = new Course { Id = 1, Name = "Course 1", StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2019, 3, 10) }; // third
            var course2 = new Course { Id = 2, Name = "Course 1", StartDate = new DateTime(2019, 7, 10), EndDate = new DateTime(2019, 7, 20) }; // second
            var course3 = new Course { Id = 3, Name = "Course 1", StartDate = new DateTime(2019, 7, 10), EndDate = new DateTime(2019, 8, 10) }; // first

            var user = new User { Id = UserIdValid };
            user.Courses.Add(new StudentCourse { CourseId = course1.Id, Grade = Grade.A });
            user.Courses.Add(new StudentCourse { CourseId = course2.Id, Grade = Grade.B });
            user.Courses.Add(new StudentCourse { CourseId = course3.Id, Grade = null });

            var certificate1 = new Certificate { Id = "1", CourseId = course1.Id, StudentId = UserIdValid };
            var certificate2 = new Certificate { Id = "2", CourseId = course2.Id, StudentId = UserIdValid };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3);
            await db.Users.AddAsync(user);
            await db.Certificates.AddRangeAsync(certificate1, certificate2);
            await db.SaveChangesAsync();

            return db;
        }

        private IUserService InitializeUserService(LearningSystemDbContext db)
            => new UserService(db, Tests.Mapper);
    }
}
