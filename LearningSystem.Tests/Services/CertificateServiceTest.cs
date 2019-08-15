namespace LearningSystem.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Certificates;
    using Moq;
    using Xunit;

    public class CertificateServiceTest
    {
        private const int CourseValidId = 10;
        private const int CourseInvalidId = 20;

        private const string CourseName = "Course name";

        private const string CertificateIdValid = "CertificateValid";
        private const string CertificateIdInvalid = "CertificateInvalid";

        private const string StudentEnrolledId = "StudentEnrolledId";
        private const string StudentNotEnrolledId = "StudentNotEnrolledId";

        private const string TrainerValidId = "TrainerValidId";
        private const string TrainerInvalidId = "TrainerInvalidId";
        private const string UserIdValid = "UserValid";

        private const Grade CertificateGrade = Grade.A;

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_GivenInvalidCourseTrainer()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultInvalidCourse = await certificateService.CreateAsync(TrainerValidId, CourseInvalidId, It.IsAny<string>(), It.IsAny<Grade>());
            var resultInvalidTrainer = await certificateService.CreateAsync(TrainerInvalidId, CourseValidId, It.IsAny<string>(), It.IsAny<Grade>());

            // Assert
            Assert.False(resultInvalidCourse);
            Assert.False(resultInvalidTrainer);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_GivenStudentNotEnrolledInCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.CreateAsync(TrainerValidId, CourseValidId, StudentNotEnrolledId, It.IsAny<Grade>());

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(Grade.D)]
        [InlineData(Grade.E)]
        [InlineData(Grade.F)]
        public async Task CreateAsync_ShouldReturnFalse_GivenInvalidGrade(Grade invalidGrade)
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, invalidGrade);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_GivenExistingGradeHasNotImproved()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);
            await this.PrepareStudentCourseCertificate(db, Grade.B);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultSameGrade = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, Grade.B);
            var resultWorseGrade = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, Grade.C);

            // Assert
            Assert.False(resultSameGrade);
            Assert.False(resultWorseGrade);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTrue_GivenFirstValidGrade()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);

            var certificateService = this.InitializeCertificateService(db);
            var certificatesCountBefore = this.GetCertificatesCountForValidStudentCourse(db);

            // Act
            var result = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, CertificateGrade);

            var certificatesCountAfter = this.GetCertificatesCountForValidStudentCourse(db);
            var certificate = this.GetLastCertificateForValidStudentCourse(db);

            // Assert
            Assert.True(result);
            Assert.Equal(1, certificatesCountAfter - certificatesCountBefore);
            AssertCertificate(certificate);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTrue_ExistingGradeHasImproved()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);
            await this.PrepareStudentCourseCertificate(db, Grade.B);

            var certificateService = this.InitializeCertificateService(db);
            var certificatesCountBefore = this.GetCertificatesCountForValidStudentCourse(db);

            // Act
            var result = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, CertificateGrade);

            var certificatesCountAfter = this.GetCertificatesCountForValidStudentCourse(db);
            var certificate = this.GetLastCertificateForValidStudentCourse(db);

            // Assert
            Assert.True(result);
            Assert.Equal(1, certificatesCountAfter - certificatesCountBefore);
            AssertCertificate(certificate);
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnNull_GivenInvalidCertificate()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.DownloadAsync(CertificateIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnCorrectData_GivenValidCertificate()
        {
            // Arrange
            var db = Tests.InitializeDatabase();

            var student = new User { Id = UserIdValid, Name = "Student name" };
            var trainer = new User { Id = TrainerValidId, Name = "Trainer name" };
            var course = new Course
            {
                Id = CourseValidId,
                Name = CourseName,
                StartDate = new DateTime(2019, 7, 1),
                EndDate = new DateTime(2019, 8, 10),
                TrainerId = TrainerValidId,
            };
            var certificate = new Certificate
            {
                Id = CertificateIdValid,
                IssueDate = DateTime.UtcNow,
                Grade = Grade.A,
                StudentId = UserIdValid,
                CourseId = CourseValidId
            };

            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddAsync(course);
            await db.Certificates.AddAsync(certificate);
            await db.SaveChangesAsync();

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.DownloadAsync(CertificateIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CertificateServiceModel>(result);

            Assert.Equal(certificate.Id, result.Id);
            Assert.Equal(certificate.Grade, result.Grade);
            Assert.Equal(certificate.IssueDate, result.IssueDate);

            Assert.Equal(course.Name, result.CourseName);
            Assert.Equal(course.StartDate, result.CourseStartDate);
            Assert.Equal(course.EndDate, result.CourseEndDate);

            Assert.Equal(student.Name, result.StudentName);
            Assert.Equal(trainer.Name, result.CourseTrainerName);

            Assert.Null(result.DownloadUrl); // to be set in controller
        }

        [Theory]
        [InlineData(Grade.D, false)]
        [InlineData(Grade.E, false)]
        [InlineData(Grade.F, false)]
        [InlineData(null, false)]
        public void IsGradeEligibleForCertificate_ShouldReturnFalse_WithGradesDEForNull(Grade? grade, bool expectedResult)
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = certificateService.IsGradeEligibleForCertificate(grade);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsGradeEligibleForCertificate_ShouldReturnTrue_WithGradesInRangeAtoC()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = certificateService.IsGradeEligibleForCertificate(
                It.IsInRange(Grade.A, Grade.C, Range.Inclusive));

            // Assert
            Assert.True(result);
        }

        private static void AssertCertificate(Certificate certificate)
        {
            var dateTimeUtcNow = DateTime.UtcNow;

            Assert.Equal(StudentEnrolledId, certificate.StudentId);
            Assert.Equal(CourseValidId, certificate.CourseId);
            Assert.Equal(CertificateGrade, certificate.Grade);

            Assert.Equal(dateTimeUtcNow.Year, certificate.IssueDate.Year);
            Assert.Equal(dateTimeUtcNow.Month, certificate.IssueDate.Month);
            Assert.Equal(dateTimeUtcNow.Day, certificate.IssueDate.Day);
        }

        private Certificate GetLastCertificateForValidStudentCourse(LearningSystemDbContext db)
           => db.Certificates
           .Where(c => c.StudentId == StudentEnrolledId)
           .Where(c => c.CourseId == CourseValidId)
           .LastOrDefault();

        private int GetCertificatesCountForValidStudentCourse(LearningSystemDbContext db)
            => db.Certificates
            .Where(c => c.StudentId == StudentEnrolledId)
            .Where(c => c.CourseId == CourseValidId)
            .Count();

        private async Task PrepareStudentCourseCertificate(LearningSystemDbContext db, Grade grade)
        {
            await db.Certificates.AddAsync(new Certificate
            {
                StudentId = StudentEnrolledId,
                CourseId = CourseValidId,
                Grade = grade
            });

            await db.SaveChangesAsync();
        }

        private async Task PrepareStudentInCourse(LearningSystemDbContext db)
        {
            var student = new User { Id = StudentEnrolledId };
            var trainer = new User { Id = TrainerValidId };

            var course = new Course { Id = CourseValidId, TrainerId = TrainerValidId };
            course.Students.Add(new StudentCourse { StudentId = StudentEnrolledId });

            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddAsync(course);

            await db.SaveChangesAsync();
        }

        private ICertificateService InitializeCertificateService(LearningSystemDbContext db)
          => new CertificateService(db, Tests.Mapper);
    }
}
