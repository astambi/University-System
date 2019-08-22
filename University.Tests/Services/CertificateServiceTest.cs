namespace University.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Certificates;
    using Xunit;

    public class CertificateServiceTest
    {
        private const int CourseValidId = 10;
        private const int CourseInvalidId = 20;

        private const string CourseName = "Course name";

        private const string CertificateIdValid = "CertificateValid";
        private const string CertificateIdInvalid = "CertificateInvalid";

        private const decimal CertificateGrade = 6.00m;

        private const decimal GradeSame = 5.50m;
        private const decimal GradeWorse = 5.49m;

        private const string StudentEnrolledId = "StudentEnrolledId";
        private const string StudentNotEnrolledId = "StudentNotEnrolledId";

        private const string TrainerValidId = "TrainerValidId";
        private const string TrainerInvalidId = "TrainerInvalidId";
        private const string UserIdValid = "UserValid";

        [Fact]
        public async Task CreateAsync_ShouldReturnFalse_GivenInvalidCourseTrainer()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultInvalidCourse = await certificateService.CreateAsync(TrainerValidId, CourseInvalidId, It.IsAny<string>(), It.IsAny<decimal>());
            var resultInvalidTrainer = await certificateService.CreateAsync(TrainerInvalidId, CourseValidId, It.IsAny<string>(), It.IsAny<decimal>());

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
            var result = await certificateService.CreateAsync(TrainerValidId, CourseValidId, StudentNotEnrolledId, It.IsAny<decimal>());

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(4.99)]
        [InlineData(6.01)]
        public async Task CreateAsync_ShouldReturnFalse_GivenInvalidGrade(decimal invalidGrade)
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


            await this.PrepareStudentCourseCertificate(db, GradeSame);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultSameGrade = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, GradeSame);
            var resultWorseGrade = await certificateService.CreateAsync(
                TrainerValidId, CourseValidId, StudentEnrolledId, GradeWorse);

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
        public async Task CreateAsync_ShouldReturnTrue_GivenExistingGradeHasImproved()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);
            await this.PrepareStudentCourseCertificate(db, GradeWorse);

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
        public async Task RemovedAsync_ShouldReturnFalse_GivenInvalidCertificate()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.RemoveAsync(CertificateIdInvalid, It.IsAny<string>(), It.IsAny<int>());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemovedAsync_ShouldReturnFalse_GivenInvalidTrainerCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);
            await this.PrepareStudentCourseCertificate(db, 6.00m);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultInvalidTrainer = await certificateService.RemoveAsync(CertificateIdValid, TrainerInvalidId, CourseValidId);
            var resultInvalidCourse = await certificateService.RemoveAsync(CertificateIdValid, TrainerValidId, CourseInvalidId);

            // Assert
            Assert.False(resultInvalidTrainer);
            Assert.False(resultInvalidCourse);
        }

        [Fact]
        public async Task RemovedAsync_ShouldRemoveCertificate_GivenValidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await this.PrepareStudentInCourse(db);
            await this.PrepareStudentCourseCertificate(db, 6.00m);

            var certificateService = this.InitializeCertificateService(db);

            // Act
            var result = await certificateService.RemoveAsync(CertificateIdValid, TrainerValidId, CourseValidId);
            var certificate = db.Certificates.Find(CertificateIdValid);

            // Assert
            Assert.True(result);
            Assert.Null(certificate);
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
                GradeBg = DataConstants.GradeBgMaxValue,
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
            Assert.Equal(certificate.GradeBg, result.GradeBg);
            Assert.Equal(certificate.IssueDate, result.IssueDate);

            Assert.Equal(course.Name, result.CourseName);
            Assert.Equal(course.StartDate, result.CourseStartDate);
            Assert.Equal(course.EndDate, result.CourseEndDate);

            Assert.Equal(student.Name, result.StudentName);
            Assert.Equal(trainer.Name, result.CourseTrainerName);

            Assert.Null(result.DownloadUrl); // to be set in controller
        }

        [Fact]
        public void IsGradeEligibleForCertificate_ShouldReturnFalse_WithInvalidGrade()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultInvalidMin = certificateService.IsGradeEligibleForCertificate(DataConstants.GradeBgCertificateMinValue - 0.01m);
            var resultInvalidMax = certificateService.IsGradeEligibleForCertificate(DataConstants.GradeBgMaxValue + 0.01m);

            // Assert
            Assert.False(resultInvalidMin);
            Assert.False(resultInvalidMax);
        }

        [Fact]
        public void IsGradeEligibleForCertificate_ShouldReturnTrue_WithValidGrades()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var certificateService = this.InitializeCertificateService(db);

            // Act
            var resultMin = certificateService.IsGradeEligibleForCertificate(DataConstants.GradeBgCertificateMinValue);
            var resultMid = certificateService.IsGradeEligibleForCertificate(5.50m);
            var resultMax = certificateService.IsGradeEligibleForCertificate(DataConstants.GradeBgMaxValue);

            // Assert
            Assert.True(resultMin);
            Assert.True(resultMid);
            Assert.True(resultMax);
        }

        private static void AssertCertificate(Certificate certificate)
        {
            var dateTimeUtcNow = DateTime.UtcNow;

            Assert.Equal(StudentEnrolledId, certificate.StudentId);
            Assert.Equal(CourseValidId, certificate.CourseId);
            Assert.Equal(CertificateGrade, certificate.GradeBg);

            Assert.Equal(dateTimeUtcNow.Year, certificate.IssueDate.Year);
            Assert.Equal(dateTimeUtcNow.Month, certificate.IssueDate.Month);
            Assert.Equal(dateTimeUtcNow.Day, certificate.IssueDate.Day);
        }

        private Certificate GetLastCertificateForValidStudentCourse(UniversityDbContext db)
           => db.Certificates
           .Where(c => c.StudentId == StudentEnrolledId)
           .Where(c => c.CourseId == CourseValidId)
           .LastOrDefault();

        private int GetCertificatesCountForValidStudentCourse(UniversityDbContext db)
            => db.Certificates
            .Where(c => c.StudentId == StudentEnrolledId)
            .Where(c => c.CourseId == CourseValidId)
            .Count();

        private async Task PrepareStudentCourseCertificate(UniversityDbContext db, decimal grade)
        {
            await db.Certificates.AddAsync(new Certificate
            {
                Id = CertificateIdValid,
                StudentId = StudentEnrolledId,
                CourseId = CourseValidId,
                GradeBg = grade
            });

            await db.SaveChangesAsync();
        }

        private async Task PrepareStudentInCourse(UniversityDbContext db)
        {
            var student = new User { Id = StudentEnrolledId };
            var trainer = new User { Id = TrainerValidId };

            var course = new Course { Id = CourseValidId, TrainerId = TrainerValidId };
            course.Students.Add(new StudentCourse { StudentId = StudentEnrolledId });

            await db.Users.AddRangeAsync(student, trainer);
            await db.Courses.AddAsync(course);

            await db.SaveChangesAsync();
        }

        private ICertificateService InitializeCertificateService(UniversityDbContext db)
          => new CertificateService(db, Tests.Mapper);
    }
}
