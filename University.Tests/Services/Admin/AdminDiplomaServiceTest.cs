namespace University.Tests.Services.Admin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin;
    using University.Services.Admin.Implementations;
    using Xunit;

    public class AdminDiplomaServiceTest
    {
        private const int CurriculumCoveredWithDiploma = 101;
        private const int CurriculumCoveredWithoutDiploma = 202;
        private const int CurriculumNotCovered = 303;
        private const int CurriculumValidId = 1;
        private const int CurriculumInvalidId = 11;

        private const string DiplomaValidId = "AAA";
        private const string DiplomaValidId2 = "BBB";
        private const string DiplomaInvalidId = "XXX";

        private const string StudentValidId = "Student AAA";
        private const string StudentInvalidId = "Student XXX";

        [Theory]
        [InlineData(null)]
        [InlineData(DiplomaInvalidId)]
        public async Task ExistsAsync_ShouldReturnFalse_GivenInvalidCurriculum(string diplomaInvalidId)
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.ExistsAsync(diplomaInvalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_GivenValidCurriculum()
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.ExistsAsync(DiplomaValidId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(CurriculumInvalidId, StudentValidId)]
        [InlineData(CurriculumValidId, StudentInvalidId)]
        public async Task ExistsForCurriculumStudentAsync_ShouldReturnFalse_GivenInvalidInput(int curriculumId, string studentId)
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var resultInvalid = await adminDiplomaService.ExistsForCurriculumStudentAsync(curriculumId, studentId);

            // Assert
            Assert.False(resultInvalid);
        }

        [Fact]
        public async Task ExistsForCurriculumStudentAsync_ShouldReturnTrue_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.ExistsForCurriculumStudentAsync(CurriculumValidId, StudentValidId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(DiplomaInvalidId)]
        public async Task RemoveAsync_ShouldReturnFalse_GivenInvalidInput(string diplomaInvalidId)
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.RemoveAsync(diplomaInvalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveCorrectEntity_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareDiplomas();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.RemoveAsync(DiplomaValidId);
            var diploma = db.Diplomas.Find(DiplomaValidId);

            // Assert
            Assert.True(result);
            Assert.Null(diploma);
        }

        [Theory]
        [InlineData(CurriculumInvalidId, StudentValidId)]
        [InlineData(CurriculumValidId, StudentInvalidId)]
        public async Task HasPassedAllCurriculumCoursesAsync_ShouldReturnFalse_GivenInvalidCurriculumOrStudent(int curriculumId, string studentId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCoursesPassed();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.HasPassedAllCurriculumCoursesAsync(curriculumId, studentId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasPassedAllCurriculumCoursesAsync_ShouldReturnFalse_GivenMissingCoursesPassed()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCoursesPassed();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.HasPassedAllCurriculumCoursesAsync(CurriculumNotCovered, StudentValidId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(CurriculumCoveredWithoutDiploma, StudentValidId)]
        [InlineData(CurriculumCoveredWithDiploma, StudentValidId)]
        public async Task HasPassedAllCurriculumCoursesAsync_ShouldReturnTrue_GivenAllCoursesPassed_WithOrWithoutDiploma(int curriculumId, string studentId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCoursesPassed();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            // Act
            var result = await adminDiplomaService.HasPassedAllCurriculumCoursesAsync(curriculumId, studentId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(CurriculumValidId, StudentInvalidId)] // invalid
        [InlineData(CurriculumInvalidId, StudentValidId)] // invalid
        [InlineData(CurriculumNotCovered, StudentValidId)] // missing required certificates
        [InlineData(CurriculumCoveredWithDiploma, StudentValidId)] // existing curriculum diploma 
        public async Task CreateAsync_ShouldNotSaveDiploma_GivenInvalidInput_OrMissingCourseCertiticates_OrExistingDiploma(int curriculumId, string studentId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCoursesPassed();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            var diplomasCountBefore = db.Diplomas.Count();

            // Act
            var result = await adminDiplomaService.CreateAsync(curriculumId, studentId);
            var diplomasCountAfter = db.Diplomas.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(0, diplomasCountAfter - diplomasCountBefore);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidCriteria()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCoursesPassed();
            var adminDiplomaService = this.InitializeDiplomaService(db);

            var diplomasCountBefore = db.Diplomas.Count();

            // Act
            var result = await adminDiplomaService.CreateAsync(CurriculumCoveredWithoutDiploma, StudentValidId);

            var diplomasCountAfter = db.Diplomas.Count();
            var resultDiploma = db
                .Diplomas
                .Where(d => d.CurriculumId == CurriculumCoveredWithoutDiploma)
                .Where(d => d.StudentId == StudentValidId)
                .FirstOrDefault();

            // Assert
            Assert.True(result);

            Assert.Equal(1, diplomasCountAfter - diplomasCountBefore);

            Assert.NotNull(resultDiploma);
            Assert.Equal(CurriculumCoveredWithoutDiploma, resultDiploma.CurriculumId);
            Assert.Equal(StudentValidId, resultDiploma.StudentId);

            var now = DateTime.UtcNow;
            Assert.Equal(now.Year, resultDiploma.IssueDate.Year);
            Assert.Equal(now.Month, resultDiploma.IssueDate.Month);
            Assert.Equal(now.Day, resultDiploma.IssueDate.Day);
        }

        private async Task<UniversityDbContext> PrepareCurriculumsWithCoursesPassed()
        {
            var student = new User { Id = StudentValidId };

            var course1Passed = new Course { Id = 101 };
            var course2Passed = new Course { Id = 202 };
            var course3NotPassed = new Course { Id = 303 };

            var certificateCourse1 = new Certificate { Id = "certificateCourse1", StudentId = student.Id, CourseId = course1Passed.Id };
            var certificateCourse2 = new Certificate { Id = "certificateCourse2", StudentId = student.Id, CourseId = course2Passed.Id };

            // Eligible & Existing diploma
            var curriculum1 = new Curriculum { Id = CurriculumCoveredWithDiploma };
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course1Passed.Id });

            var diplomaCurriculum1 = new Diploma { Id = "diplomaCurriculum1", StudentId = student.Id, CurriculumId = curriculum1.Id };

            // Eligible => all required courses passed & no diploma
            var curriculum2 = new Curriculum { Id = CurriculumCoveredWithoutDiploma };
            curriculum2.Courses.Add(new CurriculumCourse { CourseId = course1Passed.Id });
            curriculum2.Courses.Add(new CurriculumCourse { CourseId = course2Passed.Id });

            // Not eligible => Missing courses
            var curriculum3 = new Curriculum { Id = CurriculumNotCovered };
            curriculum3.Courses.Add(new CurriculumCourse { CourseId = course1Passed.Id });
            curriculum3.Courses.Add(new CurriculumCourse { CourseId = course2Passed.Id });
            curriculum3.Courses.Add(new CurriculumCourse { CourseId = course3NotPassed.Id });

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(student);
            await db.Courses.AddRangeAsync(course1Passed, course2Passed, course3NotPassed);
            await db.Certificates.AddRangeAsync(certificateCourse1, certificateCourse2);
            await db.Curriculums.AddRangeAsync(curriculum1, curriculum2, curriculum3);
            await db.Diplomas.AddRangeAsync(diplomaCurriculum1);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareDiplomas()
        {
            var diploma1 = new Diploma { Id = DiplomaValidId, StudentId = StudentValidId, CurriculumId = CurriculumValidId };
            var diploma2 = new Diploma { Id = DiplomaValidId2, StudentId = "2", CurriculumId = 2 };

            var curriculum1 = new Curriculum { Id = CurriculumValidId };
            var curriculum2 = new Curriculum { Id = CurriculumInvalidId };

            var user1 = new User { Id = StudentValidId };
            var user2 = new User { Id = StudentInvalidId };

            var db = Tests.InitializeDatabase();
            await db.Curriculums.AddRangeAsync(curriculum1, curriculum2);
            await db.Diplomas.AddRangeAsync(diploma1, diploma2);
            await db.Users.AddRangeAsync(user1, user2);
            await db.SaveChangesAsync();

            return db;
        }

        private IAdminDiplomaService InitializeDiplomaService(UniversityDbContext db)
            => new AdminDiplomaService(db);
    }
}
