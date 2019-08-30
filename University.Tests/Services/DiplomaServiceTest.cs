namespace University.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Diplomas;
    using Xunit;

    public class DiplomaServiceTest
    {
        private const int CurriculumId = 1;
        private const string CurriculumName = "C# Web developer";

        private const string DiplomaInvalid = "InvalidDiplomaId";
        private const string DiplomaValid = "ValidDiplomaId";

        private const string StudentId = "StudentId";
        private const string StudentName = "Student Name";

        private readonly DateTime IssueDate = new DateTime(2019, 8, 15);

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var diplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await diplomaService.GetByIdAsync(DiplomaInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var diplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await diplomaService.GetByIdAsync(DiplomaValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DiplomaServiceModel>(result);

            Assert.Equal(DiplomaValid, result.Id);
            Assert.Equal(this.IssueDate, result.IssueDate);
            Assert.Equal(CurriculumName, result.CurriculumName);
            Assert.Equal(StudentId, result.StudentId);
            Assert.Equal(StudentName, result.StudentName);
        }

        private async Task<UniversityDbContext> PrepareDiplomas()
        {
            var db = Tests.InitializeDatabase();

            var curriculum = new Curriculum { Id = CurriculumId, Name = CurriculumName };
            var diploma = new Diploma { Id = DiplomaValid, IssueDate = IssueDate, CurriculumId = CurriculumId, StudentId = StudentId };
            var student = new User { Id = StudentId, Name = StudentName };

            await db.Curriculums.AddAsync(curriculum);
            await db.Diplomas.AddAsync(diploma);
            await db.Users.AddAsync(student);
            await db.SaveChangesAsync();

            return db;
        }

        private IDiplomaService InitializeDiplomaService(UniversityDbContext db)
            => new DiplomaService(db, Tests.Mapper);
    }
}
