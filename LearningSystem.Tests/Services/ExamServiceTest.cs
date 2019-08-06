namespace LearningSystem.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Tests.Mocks;
    using Moq;
    using Xunit;

    public class ExamServiceTest
    {
        private const int CourseInvalid = -100;
        private const int CourseValid = 1;
        private const string StudentEnrolled = "Enrolled";
        private const string StudentNotEnrolled = "NotEnrolled";
        private const int Precision = 150; // ms

        [Fact]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(false);

            var examService = this.InitializeExamService(db, courseService.Object);

            // Act
            // Invalid student
            var resultInvalidUser = await examService.CreateAsync(CourseValid, StudentNotEnrolled, It.IsAny<byte[]>());
            var countInvalidStudent = db.ExamSubmissions.Count();

            // Invalid course
            var resultInvalidCourse = await examService.CreateAsync(CourseInvalid, StudentEnrolled, It.IsAny<byte[]>());
            var countInvalidCourse = db.ExamSubmissions.Count();

            // Assert
            Assert.False(resultInvalidCourse);
            Assert.False(resultInvalidUser);

            Assert.Equal(0, countInvalidCourse);
            Assert.Equal(0, countInvalidStudent);

            courseService.Verify();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenInvalidExamFile(byte[] exam)
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(true);

            var examService = this.InitializeExamService(db, courseService.Object);

            // Act
            // Invalid student
            var resultInvalidExamFile = await examService.CreateAsync(CourseValid, StudentEnrolled, exam);
            var countExams = db.ExamSubmissions.Count();

            // Assert
            Assert.False(resultInvalidExamFile);
            Assert.Equal(0, countExams);

            courseService.Verify();
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenEnrolledStudentInCourseAndValidExamFile()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();

            var courseService = CourseServiceMock.GetMock;
            courseService.IsUserEnrolledInCourseAsync(true);

            var examService = this.InitializeExamService(db, courseService.Object);

            var exam = new byte[] { 1, 1, 1 };

            // Act
            var resultValid = await examService.CreateAsync(CourseValid, StudentEnrolled, exam);
            var countValid = db.ExamSubmissions.Count();
            var examSaved = db.ExamSubmissions.FirstOrDefault(e =>
                e.StudentId == StudentEnrolled
                && e.CourseId == CourseValid);

            // Assert
            Assert.True(resultValid);
            Assert.Equal(1, countValid);
            Assert.NotNull(examSaved);

            Assert.Equal(CourseValid, examSaved.CourseId);
            Assert.Equal(StudentEnrolled, examSaved.StudentId);
            Assert.Same(exam, examSaved.FileSubmission);

            examSaved.SubmissionDate.Should().BeCloseTo(DateTime.UtcNow, Precision);

            courseService.Verify();
        }

        [Fact]
        public async Task AllByStudentCourseAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db, courseService: null);

            var examsSortedByDateDesc = db
                .ExamSubmissions
                .OrderByDescending(e => e.SubmissionDate)
                .ToList();

            // Act            
            var result = await examService.AllByStudentCourseAsync(CourseValid, StudentEnrolled);
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<ExamSubmissionServiceModel>>(result);
            Assert.Equal(2, result.Count());

            for (int i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = examsSortedByDateDesc[i];

                Assert.Equal(actual.Id, expected.Id);
                Assert.Equal(actual.SubmissionDate, expected.SubmissionDate);
            }
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

        private async Task<LearningSystemDbContext> PrepareStudentInCourseExamSubmissions()
        {
            var student = new User { Id = StudentEnrolled };
            var course = new Course { Id = CourseValid };
            course.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var exam1 = new ExamSubmission
            {
                Id = 1,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 15, 00),
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                FileSubmission = null
            };
            var exam2 = new ExamSubmission
            {
                Id = 2,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 15, 50),
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                FileSubmission = null
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddAsync(student);
            await db.Courses.AddAsync(course);
            await db.ExamSubmissions.AddRangeAsync(new List<ExamSubmission> { exam1, exam2 });
            await db.SaveChangesAsync();

            return db;
        }

        private IExamService InitializeExamService(LearningSystemDbContext db, ICourseService courseService)
            => new ExamService(db, courseService, Tests.Mapper);
    }
}
