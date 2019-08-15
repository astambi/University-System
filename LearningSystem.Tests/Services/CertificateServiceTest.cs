namespace LearningSystem.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Certificates;
    using Xunit;

    public class CertificateServiceTest
    {
        private const int CourseId = 10;
        private const string CourseName = "Course 10";

        private const string CertificateIdValid = "CertificateValid";
        private const string CertificateIdInvalid = "CertificateInvalid";

        private const string TrainerId = "TrainerId";

        private const string UserIdValid = "UserValid";

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
            var trainer = new User { Id = TrainerId, Name = "Trainer name" };
            var course = new Course
            {
                Id = CourseId,
                Name = CourseName,
                StartDate = new DateTime(2019, 7, 1),
                EndDate = new DateTime(2019, 8, 10),
                TrainerId = TrainerId,
            };
            var certificate = new Certificate
            {
                Id = CertificateIdValid,
                IssueDate = DateTime.UtcNow,
                Grade = Grade.A,
                StudentId = UserIdValid,
                CourseId = CourseId
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

        private ICertificateService InitializeCertificateService(LearningSystemDbContext db)
          => new CertificateService(db, Tests.Mapper);
    }
}
