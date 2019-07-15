namespace LearningSystem.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using LearningSystem.Common.Infrastructure.Extensions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Xunit;

    public class CourseServiceTest
    {
        private const int CourseInvalid = -100;
        private const int CourseValid = 1;
        private const string StudentEnrolled = "Enrolled";
        private const string StudentInvalid = "Invalid";
        private const string StudentNotEnrolled = "NotEnrolled";
        private const string StudentValid = "Valid";
        private const int Precision = 100; // ms

        [Fact]
        public async Task AddExamSubmissionAsync_ShouldSaveCorrectData_WithEnrolledStudentInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var courseService = this.InitializeCourseService(db);

            var exam = new byte[] { 1, 1, 1 };

            // Act
            // Invalid student
            await courseService.AddExamSubmissionAsync(CourseValid, StudentNotEnrolled, exam);
            var countInvalidStudent = db.ExamSubmissions.Count();

            // Invalid course
            await courseService.AddExamSubmissionAsync(CourseInvalid, StudentEnrolled, exam);
            var countInvalidCourse = db.ExamSubmissions.Count();

            await courseService.AddExamSubmissionAsync(CourseValid, StudentEnrolled, exam);
            var countValid = db.ExamSubmissions.Count();
            var submissionSaved = db.ExamSubmissions
                .FirstOrDefault(e => e.StudentId == StudentEnrolled
                                  && e.CourseId == CourseValid);

            // Assert
            // Invalid student or course
            countInvalidStudent.Should().Equals(0);
            countInvalidCourse.Should().Equals(0);

            // Valid exam submission data
            countValid.Should().Equals(1);
            submissionSaved.Should().NotBeNull();

            submissionSaved.CourseId.Should().Equals(CourseValid);
            submissionSaved.StudentId.Should().Equals(StudentEnrolled);
            submissionSaved.FileSubmission.Should().BeSameAs(exam);
            submissionSaved.SubmissionDate.Should().BeCloseTo(DateTime.UtcNow, Precision);
        }

        [Fact]
        public async Task AllActiveWithTrainersAsync_ShouldReturnCorrectResult_BySearchFilterAndOrder()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.AllActiveWithTrainersAsync("T");

            // Assert
            result
                .Should()
                .HaveCount(3)
                .And
                .Match(r =>
                    r.ElementAt(0).Id == 3
                    && r.ElementAt(1).Id == 2
                    && r.ElementAt(2).Id == 1);
        }

        [Fact]
        public async Task AllActiveWithTrainersAsync_ShouldReturnCorrectResult_WithPagination()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultPage1Of2 = await courseService.AllActiveWithTrainersAsync("T", 1, 2);
            var resultPage2Of2 = await courseService.AllActiveWithTrainersAsync("T", 2, 2);

            var resultPagePositiveInvalid = await courseService.AllActiveWithTrainersAsync("T", 100, 12);

            var resultPageNegative = await courseService.AllActiveWithTrainersAsync("T", int.MinValue, 12);
            var resultPageSizeNegative = await courseService.AllActiveWithTrainersAsync("T", int.MinValue, int.MinValue);

            // Assert
            resultPage1Of2
                .Should()
                .HaveCount(2)
                .And
                .Match(r => r.ElementAt(0).Id == 3
                         && r.ElementAt(1).Id == 2);

            resultPage2Of2
                .Should()
                .HaveCount(1)
                .And
                .Match(r => r.ElementAt(0).Id == 1);

            resultPagePositiveInvalid
                .Should()
                .HaveCount(0);

            resultPageNegative
               .Should()
               .HaveCount(3); // default page = 1

            resultPageSizeNegative
                .Should()
                .HaveCount(3); // default page = 1, pageSize = 12
        }

        [Fact]
        public async Task AllArchivedWithTrainersAsync_ShouldReturnCorrectResult_BySearchFilterAndOrder()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var result = await courseService.AllArchivedWithTrainersAsync("T");

            // Assert
            result
                .Should()
                .HaveCount(3)
                .And
                .Match(r =>
                    r.ElementAt(0).Id == 7
                    && r.ElementAt(1).Id == 6
                    && r.ElementAt(2).Id == 5);
        }

        [Fact]
        public async Task AllArchivedWithTrainersAsync_ShouldReturnCorrectResult_WithPagination()
        {
            // Arrange
            var db = await this.PrepareCoursesToSearch();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultPage1Of2 = await courseService.AllArchivedWithTrainersAsync("T", 1, 2);
            var resultPage2Of2 = await courseService.AllArchivedWithTrainersAsync("T", 2, 2);

            var resultPagePositiveInvalid = await courseService.AllArchivedWithTrainersAsync("T", 100, 12);

            var resultPageNegative = await courseService.AllArchivedWithTrainersAsync("T", int.MinValue, 12);
            var resultPageSizeNegative = await courseService.AllArchivedWithTrainersAsync("T", int.MinValue, int.MinValue);

            // Assert
            resultPage1Of2
                .Should()
                .HaveCount(2)
                .And
                .Match(r => r.ElementAt(0).Id == 7
                         && r.ElementAt(1).Id == 6);

            resultPage2Of2
                .Should()
                .HaveCount(1)
                .And
                .Match(r => r.ElementAt(0).Id == 5);

            resultPagePositiveInvalid
                .Should()
                .HaveCount(0);

            resultPageNegative
               .Should()
               .HaveCount(3); // default page = 1

            resultPageSizeNegative
                .Should()
                .HaveCount(3); // default page = 1, pageSize = 12
        }

        [Fact]
        public async Task CancellUserEnrollmentInCourseAsync_ShouldRemoveValidStudentCourse_BeforeStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            // Invalid course start date
            await courseService.CancellUserEnrollmentInCourseAsync(5, StudentEnrolled);
            var studentInCourseOnCancellationAfterStartDate = db.Find<StudentCourse>(StudentEnrolled, 5);

            // Valid course start date
            await courseService.CancellUserEnrollmentInCourseAsync(4, StudentEnrolled);
            var studentInCourseOnCancellationBeforeStartDate = db.Find<StudentCourse>(StudentEnrolled, 4);

            // Assert
            studentInCourseOnCancellationAfterStartDate.Should().NotBeNull();
            studentInCourseOnCancellationBeforeStartDate.Should().BeNull();
        }

        [Fact]
        public async Task CanEnrollAsync_ShouldReturnTrue_BeforeStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultAfterStartDate = await courseService.CanEnrollAsync(1);
            var resultOnStartDate = await courseService.CanEnrollAsync(2);
            var resultBeforeStartDate = await courseService.CanEnrollAsync(3);

            // Assert
            resultAfterStartDate.Should().BeFalse();
            resultOnStartDate.Should().BeFalse();
            resultBeforeStartDate.Should().BeTrue();
        }

        [Fact]
        public async Task EnrollStudentInCourseAsync_ShouldAddValidStudentCourse_BeforeStartDate()
        {
            // Arrange
            var db = await this.PrepareCoursesToEnroll();
            var courseService = this.InitializeCourseService(db);

            // Act
            await courseService.EnrollUserInCourseAsync(CourseInvalid, StudentNotEnrolled);
            await courseService.EnrollUserInCourseAsync(4, null);
            await courseService.EnrollUserInCourseAsync(4, StudentInvalid);

            await courseService.EnrollUserInCourseAsync(1, StudentNotEnrolled);
            await courseService.EnrollUserInCourseAsync(2, StudentNotEnrolled);

            await courseService.EnrollUserInCourseAsync(3, StudentEnrolled);
            await courseService.EnrollUserInCourseAsync(3, StudentNotEnrolled);
            await courseService.EnrollUserInCourseAsync(4, StudentNotEnrolled);

            // Assert
            // Invalid student or course
            db.Find<StudentCourse>(StudentNotEnrolled, CourseInvalid).Should().BeNull();
            db.Find<StudentCourse>(null, 4).Should().BeNull();
            db.Find<StudentCourse>(StudentInvalid, 4).Should().BeNull();

            // Invalid course start date
            db.Find<StudentCourse>(StudentNotEnrolled, 1).Should().BeNull();
            db.Find<StudentCourse>(StudentNotEnrolled, 2).Should().BeNull();

            // Valid course & student not enrolled
            db.Find<StudentCourse>(StudentEnrolled, 3).Should().NotBeNull();
            db.Find<StudentCourse>(StudentNotEnrolled, 3).Should().NotBeNull();
            db.Find<StudentCourse>(StudentNotEnrolled, 4).Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_WithValidId()
        {
            // Arrange
            var db = await this.PrepareCoursesWithDetails();
            var courseService = this.InitializeCourseService(db);

            var courseId = 4;

            // Act
            var invalidCourse = await courseService.GetByIdAsync(CourseInvalid);
            var validCourse = await courseService.GetByIdAsync(courseId);

            // Assert
            invalidCourse.Should().BeNull();

            validCourse
                .Should()
                .NotBeNull()
                .And
                .BeOfType(typeof(CourseDetailsServiceModel));

            validCourse.Course
                .Should()
                .NotBeNull()
                .And
                .BeOfType(typeof(CourseWithDescriptionServiceModel));

            validCourse.Trainer
                .Should()
                .NotBeNull()
                .And
                .BeOfType(typeof(UserServiceModel));

            validCourse.Students
                .Should()
                .BeOfType(typeof(int))
                .And
                .Equals(4);

            validCourse.Course.Name.Should().Equals("Course-4");
            validCourse.Course.Description.Should().BeNullOrWhiteSpace();
            validCourse.Course.StartDate.Should().Equals(DateTime.Now.ToStartDateUtc().AddDays(4 + 1));
            validCourse.Course.EndDate.Should().Equals(DateTime.Now.ToEndDateUtc().AddDays(4 + 5));

            validCourse.Trainer.Id.Should().Equals(4);
            validCourse.Trainer.Name.Should().Equals("Name-4");
            validCourse.Trainer.Username.Should().Equals("Username-4");
            validCourse.Trainer.Email.Should().Equals("Email-4@gmail.com");
        }

        [Fact]
        public async Task IsUserEnrolledInCourseAsync_ShouldReturnCorrectResult()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var courseService = this.InitializeCourseService(db);

            // Act
            var resultValid = await courseService.IsUserEnrolledInCourseAsync(CourseValid, StudentEnrolled);
            var resultInvalid = await courseService.IsUserEnrolledInCourseAsync(CourseValid, StudentNotEnrolled);

            // Assert
            resultValid.Should().BeTrue();
            resultInvalid.Should().BeFalse();
        }

        private async Task<LearningSystemDbContext> PrepareCoursesToEnroll()
        {
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();

            var studentEnrolled = new User { Id = StudentEnrolled };
            var studentNotEnrolled = new User { Id = StudentNotEnrolled };

            var courses = new List<Course>
            {
                new Course{Id = 1, StartDate = startDate.AddDays(-1) }, // past
                new Course{Id = 2, StartDate = startDate.AddDays(0) },  // past
                new Course{Id = 3, StartDate = startDate.AddDays(1) },  // valid date
                new Course{Id = 4, StartDate = startDate.AddDays(1) },  // valid date, enrolled
                new Course{Id = 5, StartDate = startDate.AddDays(0) },  // past date, enrolled
            };

            studentEnrolled.Courses.Add(new StudentCourse { CourseId = 4 });
            studentEnrolled.Courses.Add(new StudentCourse { CourseId = 5 });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(courses);
            await db.Users.AddRangeAsync(studentEnrolled, studentNotEnrolled);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<LearningSystemDbContext> PrepareCoursesToSearch()
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

        private async Task<LearningSystemDbContext> PrepareCoursesWithDetails()
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

            //Courses
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

        private async Task<LearningSystemDbContext> PrepareStudentInCourse()
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

        private ICourseService InitializeCourseService(LearningSystemDbContext db)
            => new CourseService(db, Tests.Mapper);
    }
}
