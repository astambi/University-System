﻿namespace LearningSystem.Tests.Services
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
    using LearningSystem.Services.Models.Users;
    using Moq;
    using Xunit;

    public class ExamServiceTest
    {
        private const int CourseValid = 1;
        private const int CourseInvalid = 100;

        private const int ExamValid = 5;
        private const int ExamInvalid = 55;

        private const string StudentEnrolled = "Enrolled";
        private const string StudentNotEnrolled = "NotEnrolled";

        private const int Precision = 150; // ms

        [Fact]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

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
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenInvalidExamFile(byte[] exam)
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

            // Act
            // Invalid student
            var resultInvalidExamFile = await examService.CreateAsync(CourseValid, StudentEnrolled, exam);
            var countExams = db.ExamSubmissions.Count();

            // Assert
            Assert.False(resultInvalidExamFile);
            Assert.Equal(0, countExams);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenEnrolledStudentInCourseAndValidExamFile()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

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
        }

        [Fact]
        public async Task AllByStudentCourseAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

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

            for (var i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = examsSortedByDateDesc[i];

                Assert.Equal(actual.Id, expected.Id);
                Assert.Equal(actual.SubmissionDate, expected.SubmissionDate);
            }
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnNull_GivenInvalidExamOrInvalidStudent()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var resultInvalidExam = await examService.DownloadForStudentAsync(ExamInvalid, StudentEnrolled);
            var resultInvalidStudent = await examService.DownloadForStudentAsync(ExamValid, StudentNotEnrolled);

            // Assert
            Assert.Null(resultInvalidExam);
            Assert.Null(resultInvalidStudent);
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnCorrectData_GivenValidExam()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            var exam = db.ExamSubmissions.FirstOrDefault(e => e.Id == ExamValid);
            var student = db.Users.FirstOrDefault(u => u.Id == StudentEnrolled);
            var course = db.Courses.FirstOrDefault(c => c.Id == CourseValid);

            // Act
            var result = await examService.DownloadForStudentAsync(ExamValid, StudentEnrolled);

            // Assert
            Assert.IsType<ExamDownloadServiceModel>(result);
            Assert.NotNull(result);

            Assert.Equal(exam.SubmissionDate, result.SubmissionDate);
            Assert.Equal(exam.FileSubmission, result.FileSubmission);

            Assert.Equal(course.Name, result.CourseName);
            Assert.Equal(student.UserName, result.StudentUserName);
        }

        [Fact]
        public async Task ExistsForStudentAsync_ShouldReturnFalse_GivenInvalidExamOrStudent()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var resultInvalidUser = await examService.ExistsForStudentAsync(ExamValid, StudentNotEnrolled);
            var resultInvalidExam = await examService.ExistsForStudentAsync(ExamInvalid, StudentEnrolled);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidExam);
        }

        [Fact]
        public async Task ExistsForStudentAsync_ShouldReturnTrue_GivenValidExamAndStudent()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var resultValid = await examService.ExistsForStudentAsync(ExamValid, StudentEnrolled);

            // Assert
            Assert.True(resultValid);
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
            var student = new User { Id = StudentEnrolled, UserName = "UserName" };
            var course = new Course { Id = CourseValid, Name = "CourseName" };
            course.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var studentNotEnrolled = new User { Id = StudentNotEnrolled, UserName = "UserName Not Enrolled" };
            var courseOther = new Course { Id = CourseInvalid, Name = "CourseName Other" };

            var exam1 = new ExamSubmission
            {
                Id = ExamValid,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 15, 00),
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                FileSubmission = new byte[] { 1, 2, 3, 4, 5 }
            };
            var exam2 = new ExamSubmission
            {
                Id = 2,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 15, 50),
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                FileSubmission = new byte[] { 111, 27, 35 }
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student, studentNotEnrolled);
            await db.Courses.AddRangeAsync(course, courseOther);
            await db.ExamSubmissions.AddRangeAsync(new List<ExamSubmission> { exam1, exam2 });
            await db.SaveChangesAsync();

            return db;
        }

        private IExamService InitializeExamService(LearningSystemDbContext db)
            => new ExamService(db, Tests.Mapper);
    }
}
