namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Exams;
    using Xunit;

    public class ExamServiceTest
    {
        private const int CourseValid = 1;
        private const int CourseCurrent = 10;
        private const int CourseInvalid = 100;

        private const int ExamValid = 5;
        private const int ExamInvalid = 55;

        private const decimal ExamGradeBg = 6.00m;
        private const decimal PreviousGradeBg = 5.00m;

        private const string StudentEnrolled = "StudentEnrolled";
        private const string StudentNotEnrolled = "StudentNotEnrolled";

        private const string FileName = "FileName.zip";
        private const string FileUrl = "https://res.cloudinary.com/filename.zip";

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
        public async Task CanBeDownloadedByUserAsync_ShouldReturnFalse_GivenInvalidExam()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.CanBeDownloadedByUserAsync(ExamInvalid, It.IsAny<string>());

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(TrainerInvalid, false)]
        [InlineData(StudentNotEnrolled, false)]
        [InlineData(TrainerValid, true)]
        [InlineData(StudentEnrolled, true)]
        public async Task CanBeDownloadedByUserAsync_ShouldReturnCorrectResult_GivenValidExam(string userId, bool expectedResult)
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.CanBeDownloadedByUserAsync(ExamValid, userId);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(CourseValid, StudentNotEnrolled)]
        [InlineData(CourseInvalid, StudentEnrolled)]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenStudentNotEnrolledInCourse(int courseId, string studentId)
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.CreateAsync(courseId, studentId, It.IsAny<string>(), It.IsAny<string>());
            var examsCount = db.ExamSubmissions.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(0, examsCount);
        }

        [Theory]
        [InlineData(null, FileUrl)]
        [InlineData("  ", FileUrl)]
        [InlineData(FileName, null)]
        [InlineData(FileName, "  ")]
        public async Task CreateAsync_ShouldNotSaveEntry_GivenInvalidFileNameOrFileUrl(string fileName, string fileUrl)
        {
            // Arrange
            var db = await this.PrepareStudentInCourse();
            var examService = this.InitializeExamService(db);

            // Act
            // Invalid student
            var result = await examService.CreateAsync(CourseValid, StudentEnrolled, fileName, fileUrl);
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
            var result = await examService.CreateAsync(CourseValid, StudentEnrolled, FileName, FileUrl);

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
            Assert.Equal(FileName, examSaved.FileName);
            Assert.Equal(FileUrl, examSaved.FileUrl);

            examSaved.SubmissionDate
                .Should()
                .BeCloseTo(DateTime.UtcNow, Precision);
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
            var result = await examService.EvaluateAsync(TrainerInvalid, CourseValid, StudentEnrolled, It.IsAny<decimal>());

            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseInvalid, StudentEnrolled, It.IsAny<decimal>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task EvaluateAsyncc_ShouldReturnFalseAndNotSaveGrade_GivenCourseHasNotEnded()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseCurrent, StudentEnrolled, It.IsAny<decimal>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentNotEnrolled, It.IsAny<decimal>());
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenNoExamSubmissions()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseWithoutExamSubmissions();

            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentEnrolled, ExamGradeBg);
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Theory]
        [InlineData(1.99)]
        [InlineData(6.01)]
        public async Task EvaluateAsync_ShouldReturnFalseAndNotSaveGrade_GivenInvalidGrade(decimal gradeBgInvalid)
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentEnrolled, gradeBgInvalid);
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.False(result);
            Assert.Equal(PreviousGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task EvaluateAsync_ShouldReturnTrueAndSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.EvaluateAsync(TrainerValid, CourseValid, StudentEnrolled, ExamGradeBg);
            var studentCourse = await db.FindAsync<StudentCourse>(StudentEnrolled, CourseValid);

            // Assert
            Assert.True(result);
            Assert.Equal(ExamGradeBg, studentCourse.GradeBg.Value);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_ShouldReturnNull_GivenInvalidExam()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.GetDownloadUrlAsync(ExamInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_ShouldReturnCorrectData_GivenValidExam()
        {
            // Arrange
            var db = await this.PrepareStudentInCourseExamSubmissions();
            var examService = this.InitializeExamService(db);

            // Act
            var result = await examService.GetDownloadUrlAsync(ExamValid);

            // Assert
            Assert.Equal(FileUrl, result);
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

        private async Task<UniversityDbContext> PrepareStudentInCourseWithoutExamSubmissions()
        {
            var pastDate = DateTime.UtcNow.AddDays(-1);
            var futureDate = DateTime.UtcNow.AddDays(1);

            var student = new User { Id = StudentEnrolled };
            var trainer = new User { Id = TrainerValid };

            var coursePast = new Course { Id = CourseValid, TrainerId = TrainerValid, EndDate = pastDate };
            coursePast.Students.Add(new StudentCourse { StudentId = StudentEnrolled, GradeBg = PreviousGradeBg });

            var courseCurrent = new Course { Id = CourseCurrent, TrainerId = TrainerValid, EndDate = futureDate };
            courseCurrent.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddRangeAsync(coursePast, courseCurrent);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareStudentInCourseExamSubmissions()
        {
            var pastDate = DateTime.UtcNow.AddDays(-2);
            var futureDate = DateTime.UtcNow.AddDays(1);

            var student = new User { Id = StudentEnrolled };
            var trainer = new User { Id = TrainerValid };

            var coursePast = new Course { Id = CourseValid, TrainerId = TrainerValid, EndDate = pastDate };
            coursePast.Students.Add(new StudentCourse { StudentId = StudentEnrolled, GradeBg = PreviousGradeBg });

            var courseCurrent = new Course { Id = CourseCurrent, TrainerId = TrainerValid, EndDate = futureDate };
            courseCurrent.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            var exam1 = new ExamSubmission
            {
                Id = ExamValid,
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 10, 14, 15, 00), // latest
                FileUrl = FileUrl
            };
            var exam2 = new ExamSubmission
            {
                Id = 2,
                CourseId = CourseValid,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 18, 00),
                FileUrl = $"{FileUrl}-2"
            };
            var exam3 = new ExamSubmission
            {
                Id = 3,
                CourseId = CourseCurrent,
                StudentId = StudentEnrolled,
                SubmissionDate = new DateTime(2019, 7, 1, 14, 20, 50),
                FileUrl = $"{FileUrl}-3"
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddRangeAsync(coursePast, courseCurrent);
            await db.ExamSubmissions.AddRangeAsync(new List<ExamSubmission> { exam1, exam2, exam3 });
            await db.SaveChangesAsync();

            return db;
        }

        private IExamService InitializeExamService(UniversityDbContext db)
            => new ExamService(db, Tests.Mapper);
    }
}
