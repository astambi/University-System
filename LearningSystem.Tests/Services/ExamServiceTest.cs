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
    using Moq;
    using Xunit;

    public class ExamServiceTest
    {
        private const int CourseValid = 1;
        private const int CourseCurrent = 10;
        private const int CourseInvalid = 100;

        private const int ExamValid = 5;
        private const int ExamInvalid = 55;

        private const Grade ExamGrade = Grade.A;
        private const Grade PreviousGrade = Grade.B;

        private const string StudentEnrolled = "StudentEnrolled";
        private const string StudentNotEnrolled = "StudentNotEnrolled";

        private const string TrainerValid = "TrainerValid";
        private const string TrainerInvalid = "TrainerInvalid";

        private const int Precision = 150; // ms

        [Fact]
        public async Task AllByStudentCourseAsync_ShouldReturnEmptyCollection_GivenInvalidStudent()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act            
            var result = await examService.AllByStudentCourseAsync(CourseValid, StudentNotEnrolled);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ExamSubmissionServiceModel>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AllByStudentCourseAsync_ShouldReturnEmptyCollection_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act            
            var result = await examService.AllByStudentCourseAsync(CourseInvalid, StudentEnrolled);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ExamSubmissionServiceModel>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AllByStudentCourseAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            var examsSortedByDateDesc = db
                .ExamSubmissions
                .Where(e => e.CourseId == CourseValid)
                .Where(e => e.StudentId == StudentEnrolled)
                .OrderByDescending(e => e.SubmissionDate)
                .ToList();

            // Act            
            var result = await examService.AllByStudentCourseAsync(CourseValid, StudentEnrolled);
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ExamSubmissionServiceModel>>(result);
            Assert.Equal(examsSortedByDateDesc.Count, result.Count());

            for (var i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = examsSortedByDateDesc[i];

                Assert.Equal(actual.Id, expected.Id);
                Assert.Equal(actual.SubmissionDate, expected.SubmissionDate);
            }
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

            // Act
            // Invalid student
            var resultInvalidStudent = await examService.CreateAsync(CourseValid, StudentNotEnrolled, It.IsAny<byte[]>());
            var examsCountInvalidStudent = db.ExamSubmissions.Count();

            // Invalid course
            var resultInvalidCourse = await examService.CreateAsync(CourseInvalid, StudentEnrolled, It.IsAny<byte[]>());
            var examsCountInvalidCourse = db.ExamSubmissions.Count();

            // Assert
            Assert.False(resultInvalidCourse);
            Assert.False(resultInvalidStudent);

            Assert.Equal(0, examsCountInvalidCourse);
            Assert.Equal(0, examsCountInvalidStudent);
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
            var result = await examService.CreateAsync(CourseValid, StudentEnrolled, exam);
            var examsCount = db.ExamSubmissions.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(0, examsCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

            var exam = new byte[] { 1, 1, 1 };

            // Act
            var result = await examService.CreateAsync(CourseValid, StudentEnrolled, exam);

            var examsCount = db.ExamSubmissions.Count();
            var examSaved = db.ExamSubmissions
                .Where(e => e.CourseId == CourseValid)
                .Where(e => e.StudentId == StudentEnrolled)
                .FirstOrDefault();

            // Assert
            Assert.True(result);
            Assert.Equal(1, examsCount);
            Assert.NotNull(examSaved);

            Assert.Equal(CourseValid, examSaved.CourseId);
            Assert.Equal(StudentEnrolled, examSaved.StudentId);
            Assert.Same(exam, examSaved.FileSubmission);

            examSaved.SubmissionDate
                .Should()
                .BeCloseTo(DateTime.UtcNow, Precision);
        }

        [Fact]
        public async Task DownloadForStudentAsync_ShouldReturnNull_GivenInvalidExam()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForStudentAsync(ExamInvalid, StudentEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForStudentAsync_ShouldReturnNull_GivenInvalidStudent()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForStudentAsync(ExamValid, StudentNotEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForStudentAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            var examExpected = db.ExamSubmissions.Find(ExamValid);

            // Act
            var result = await examService.DownloadForStudentAsync(ExamValid, StudentEnrolled);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExamDownloadServiceModel>(result);
            AssertExamDownload(examExpected, result);
        }

        [Fact]
        public async Task DownloadForTrainerAsync_ShouldReturnNull_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForTrainerAsync(TrainerValid, CourseInvalid, StudentEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForTrainerAsync_ShouldReturnNull_GivenStudentNotEnrolled()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForTrainerAsync(TrainerValid, CourseValid, StudentNotEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForTrainerAsync_ShouldReturnNull_GivenInvalidTrainer()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForTrainerAsync(TrainerInvalid, CourseValid, StudentEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForTrainerAsync_ShouldReturnNull_GivenCourseHasNotEnded()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.DownloadForTrainerAsync(TrainerValid, CourseCurrent, StudentEnrolled);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadForTrainerAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            var latestExam = db.ExamSubmissions.Find(ExamValid);

            // Act
            var result = await examService.DownloadForTrainerAsync(TrainerValid, CourseValid, StudentEnrolled);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExamDownloadServiceModel>(result);
            AssertExamDownload(latestExam, result);
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

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenInvalidTrainer()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerInvalid, CourseValid, StudentEnrolled, It.IsAny<Grade>());

            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGrade, studentCourse.Grade.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseInvalid, StudentEnrolled, It.IsAny<Grade>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGrade, studentCourse.Grade.Value);
        }

        [Fact]
        public async Task EvaluateAsyncc_ShouldReturnFalseAndNotSaveGrade_GivenCourseHasNotEnded()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseCurrent, StudentEnrolled, It.IsAny<Grade>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGrade, studentCourse.Grade.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentNotEnrolled, It.IsAny<Grade>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGrade, studentCourse.Grade.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenNoExamSubmissions()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseWithoutExamSubmissions();

            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentEnrolled, ExamGrade);
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGrade, studentCourse.Grade.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnTrueAndSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentEnrolled, ExamGrade);
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.True(result);
            Assert.Equal(ExamGrade, studentCourse.Grade.Value);
        }

        private static void AssertExamDownload(ExamSubmission expected, ExamDownloadServiceModel result)
        {
            Assert.Equal(expected.SubmissionDate, result.SubmissionDate);
            Assert.Equal(expected.FileSubmission, result.FileSubmission);
            Assert.Equal(expected.Course.Name, result.CourseName);
            Assert.Equal(expected.Student.UserName, result.StudentUserName);
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

        private async Task<LearningSystemDbContext> PrepareStudentInCourseWithoutExamSubmissions()
        {
            var pastDate = DateTime.UtcNow.AddDays(-1);
            var futureDate = DateTime.UtcNow.AddDays(1);

            var student = new User { Id = StudentEnrolled };
            var trainer = new User { Id = TrainerValid };

            var coursePast = new Course { Id = CourseValid, TrainerId = TrainerValid, EndDate = pastDate };
            coursePast.Students.Add(new StudentCourse { StudentId = StudentEnrolled, Grade = PreviousGrade });

            var courseCurrent = new Course { Id = CourseCurrent, TrainerId = TrainerValid, EndDate = futureDate };
            courseCurrent.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddRangeAsync(coursePast, courseCurrent);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<LearningSystemDbContext> PrepareStudentInCourseExamSubmissions()
        {
            var pastDate = DateTime.UtcNow.AddDays(-2);
            var futureDate = DateTime.UtcNow.AddDays(1);

            var student = new User { Id = StudentEnrolled };
            var trainer = new User { Id = TrainerValid };

            var coursePast = new Course { Id = CourseValid, TrainerId = TrainerValid, EndDate = pastDate };
            coursePast.Students.Add(new StudentCourse { StudentId = StudentEnrolled, Grade = PreviousGrade });

            var courseCurrent = new Course { Id = CourseCurrent, TrainerId = TrainerValid, EndDate = futureDate };
            courseCurrent.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var exam1 = new ExamSubmission
            {
                Id = ExamValid,
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 10, 14, 15, 00), // latest
                FileSubmission = new byte[] { 1, 2, 3 }
            };
            var exam2 = new ExamSubmission
            {
                Id = 2,
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 18, 00),
                FileSubmission = new byte[] { 3, 4, 5 }
            };
            var exam3 = new ExamSubmission
            {
                Id = 3,
                CourseId = CourseCurrent,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 20, 50),
                FileSubmission = new byte[] { 6, 7, 8 }
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddRangeAsync(coursePast, courseCurrent);
            await db.ExamSubmissions.AddRangeAsync(new List<ExamSubmission> { exam1, exam2, exam3 });
            await db.SaveChangesAsync();

            return db;
        }

        private IExamService InitializeExamService(LearningSystemDbContext db)
            => new ExamService(db, Tests.Mapper);
    }
}
