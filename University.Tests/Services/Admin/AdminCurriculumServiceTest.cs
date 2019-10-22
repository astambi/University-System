namespace University.Tests.Services.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin;
    using University.Services.Admin.Implementations;
    using University.Services.Admin.Models.Courses;
    using University.Services.Admin.Models.Curriculums;
    using University.Services.Admin.Models.Users;
    using Xunit;

    public class AdminCurriculumServiceTest
    {
        private const int CourseValidId = 1;
        private const int CourseValidId2 = 2;
        private const int CourseValidId3 = 3;

        private const int CourseValidWithoutCurriculum = 20;

        private const int CourseInvalidId = 1000;

        private const string CourseName1 = "AAA Course";
        private const string CourseName2 = "BBB Course";

        private const string CurriculumName = "C# Web Developer";
        private const string CurriculumName2 = "JavaScript Web Developer";

        private const string CurriculumDescription2 = "JavaScript Web Developer Curriculum Description";
        private const string CurriculumDescription = "C# Web Developer Curriculum Description";

        private const string CurriculumNameForm = "    C# Web Developer    ";
        private const string CurriculumDescriptionForm = "      C# Web Developer Curriculum Description    ";

        private const int CurriculumValidId = 2;
        private const int CurriculumValidId2 = 22;
        private const int CurriculumInvalidId = 20;

        private const string StudentNameA = "StudentName AAA";
        private const string StudentNameB = "StudentName BBB";
        private const string StudentNameC = "StudentName CCC";

        private readonly DateTime DateTime1 = new DateTime(2019, 8, 11);
        private readonly DateTime DateTime2 = new DateTime(2019, 8, 22);

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_GivenInvalidCurriculum()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.ExistsAsync(CurriculumInvalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_GivenValidCurriculum()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.ExistsAsync(CurriculumValidId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(CurriculumInvalidId, CourseValidId)]
        [InlineData(CurriculumValidId, CourseInvalidId)]
        public async Task ExistsCurriculumCourseAsync_ShouldReturnFalse_GivenInvalidInput(int curriculumId, int courseId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var resultInvalid = await adminCurriculumService.ExistsCurriculumCourseAsync(curriculumId, courseId);

            // Assert
            Assert.False(resultInvalid);
        }

        [Fact]
        public async Task ExistsCurriculumCourseAsync_ShouldReturnTrue_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.ExistsCurriculumCourseAsync(CurriculumValidId, CourseValidId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.GetByIdAsync(CurriculumInvalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.GetByIdAsync(CurriculumValidId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AdminCurriculumBasicServiceModel>(result);

            Assert.Equal(CurriculumValidId, result.Id);
            Assert.Equal(CurriculumName, result.Name);
            Assert.Equal(CurriculumDescription, result.Description);
        }

        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.RemoveAsync(CurriculumInvalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveCorrectEntity_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.RemoveAsync(CurriculumValidId);
            var curriculum = db.Curriculums.Find(CurriculumValidId);

            // Assert
            Assert.True(result);
            Assert.Null(curriculum);
        }

        [Theory]
        [InlineData(CurriculumInvalidId, CourseValidId)]
        [InlineData(CurriculumValidId, CourseInvalidId)]
        public async Task RemoveCourseAsync_ShouldReturnFalse_GivenInvalidInput(int curriculumId, int courseId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var resultInvalid = await adminCurriculumService.RemoveCourseAsync(curriculumId, courseId);

            // Assert
            Assert.False(resultInvalid);
        }

        [Fact]
        public async Task RemoveCourseAsync_ShouldRemoveCorrectEntity_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.RemoveCourseAsync(CurriculumValidId, CourseValidId);
            var curriculumCourse = db.Find<CurriculumCourse>(CurriculumValidId, CourseValidId);

            // Assert
            Assert.True(result);
            Assert.Null(curriculumCourse);
        }

        [Theory]
        [InlineData(CurriculumInvalidId, CourseValidId)]
        [InlineData(CurriculumValidId, CourseInvalidId)]
        public async Task AddCourseAsync_ShouldReturnFalse_GivenInvalidCurriculumOrCourse(int curriculumId, int courseId)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var resultInvalid = await adminCurriculumService.AddCourseAsync(curriculumId, courseId);
            var curriculumCourse = db.Find<CurriculumCourse>(curriculumId, courseId);

            // Assert
            Assert.False(resultInvalid);
            Assert.Null(curriculumCourse);
        }

        [Fact]
        public async Task AddCourseAsync_ShouldReturnFalse_GivenCurriculumCourseAlreadyExists()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var resultExists = await adminCurriculumService.AddCourseAsync(CurriculumValidId, CourseValidId);

            // Assert
            Assert.False(resultExists);
        }

        [Fact]
        public async Task AddCourseAsync_ShouldAddCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.AddCourseAsync(CurriculumValidId, CourseValidWithoutCurriculum);
            var curriculumCourse = db.Find<CurriculumCourse>(CurriculumValidId, CourseValidWithoutCurriculum);

            // Assert
            Assert.True(result);
            Assert.NotNull(curriculumCourse);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("   ", null)]
        [InlineData(null, "   ")]
        [InlineData("   ", "   ")]
        public async Task CreateAsync_ShouldNotCreateEntity_GivenInvalidInput(string name, string description)
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.CreateAsync(name, description);
            var curriculumsCount = db.Curriculums.Count();

            // Assert
            Assert.True(result < 0);
            Assert.Equal(0, curriculumsCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.CreateAsync(CurriculumNameForm, CurriculumDescriptionForm);
            var curriculumsCount = db.Curriculums.Count();
            var resultEntity = db.Curriculums.Find(result);

            // Assert
            Assert.True(result > 0);
            Assert.Equal(1, curriculumsCount);

            Assert.Equal(CurriculumNameForm.Trim(), resultEntity.Name);
            Assert.Equal(CurriculumDescriptionForm.Trim(), resultEntity.Description);

            Assert.Empty(resultEntity.Diplomas);
            Assert.Empty(resultEntity.Courses);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_GivenValidCurriculum()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.UpdateAsync(CurriculumInvalidId, CurriculumNameForm, CurriculumDescriptionForm);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("   ", null)]
        [InlineData(null, "   ")]
        [InlineData("   ", "   ")]
        public async Task UpdateAsync_ShouldNotChangeEntity_GivenInvalidInput(string name, string description)
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.UpdateAsync(CurriculumValidId, name, description);
            var curriculum = db.Curriculums.Find(CurriculumValidId);

            // Assert
            Assert.False(result);

            Assert.Equal(CurriculumName, curriculum.Name);
            Assert.Equal(CurriculumDescription, curriculum.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.UpdateAsync(CurriculumValidId2, CurriculumNameForm, CurriculumDescriptionForm);
            var resultEntity = db.Curriculums.Find(CurriculumValidId2);

            // Assert
            Assert.True(result);

            Assert.Equal(CurriculumNameForm.Trim(), resultEntity.Name);
            Assert.Equal(CurriculumDescriptionForm.Trim(), resultEntity.Description);
        }

        [Fact]
        public async Task AllAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCourses();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.AllAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AdminCurriculumServiceModel>>(result);

            // Assert Correct curriculum order (name ASC)
            Assert.Equal(new[] { CurriculumName, CurriculumName2 }, result.Select(c => c.Name));

            var resultList = result.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = db.Curriculums.Find(actual.Id);

                this.AssertCurriculumDetails(expected, actual);
            }

            // Assert Correct Courses by curriculum (name ASC & start date DESC)
            var curriculum1 = resultList.First();
            var curriculum2 = resultList.Last();

            Assert.Equal(new[] { CourseValidId3, CourseValidId2, CourseValidId }, curriculum1.Courses.Select(c => c.Id));
            Assert.Empty(curriculum2.Courses);

            foreach (var resultCourse in resultList.First().Courses)
            {
                var expectedCourse = db.Courses.Find(resultCourse.Id);
                this.AssertCurriculumCourse(expectedCourse, resultCourse);
            }
        }

        [Fact]
        public async Task GetDiplomaGraduatesAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithDiplomas();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.GetDiplomaGraduatesAsync(CurriculumValidId);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AdminDiplomaGraduateServiceModel>>(result);

            // Assert Correct order (student name ASC)
            Assert.Equal(new[] { StudentNameA, StudentNameB, StudentNameC },
                result.Select(d => d.Student.Name));

            var resultList = result.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = db.Diplomas.Find(actual.Id);

                this.AssertDiplomaDetails(expected, actual);
                this.AssertStudentDetails(expected.Student, actual.Student);
            }
        }

        [Fact]
        public async Task GetEligibleCandidatesWithoutDiplomasAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareCurriculumsWithCertificatesAndDiplomas();
            var adminCurriculumService = this.InitializeCurriculumService(db);

            // Act
            var result = await adminCurriculumService.GetEligibleCandidatesWithoutDiplomasAsync(CurriculumValidId);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AdminUserListingServiceModel>>(result);

            //Assert Correct order(student name ASC)
            Assert.Equal(new[] { StudentNameA, StudentNameB }, result.Select(u => u.Name));

            var resultList = result.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                var actual = resultList[i];
                var expected = db.Users.Find(actual.Id);

                this.AssertStudentDetails(expected, actual);
            }
        }

        private void AssertCurriculumCourse(Course expected, AdminCourseBasicServiceModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.StartDate, actual.StartDate);
        }

        private void AssertCurriculumDetails(Curriculum expected, AdminCurriculumServiceModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
        }

        private void AssertDiplomaDetails(Diploma expected, AdminDiplomaGraduateServiceModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.IssueDate, actual.IssueDate);
            Assert.Equal(expected.CurriculumId, actual.CurriculumId);
        }

        private void AssertStudentDetails(User expected, AdminUserListingServiceModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.UserName, actual.Username);
            Assert.Equal(expected.Email, actual.Email);
        }

        private async Task<UniversityDbContext> PrepareCurriculumsWithCourses()
        {
            var course1 = new Course { Id = CourseValidId, Name = CourseName2, StartDate = DateTime1 };
            var course2 = new Course { Id = CourseValidId2, Name = CourseName1, StartDate = DateTime1 };
            var course3 = new Course { Id = CourseValidId3, Name = CourseName1, StartDate = DateTime2 };

            var course4 = new Course { Id = CourseValidWithoutCurriculum };

            var curriculum1 = new Curriculum { Id = CurriculumValidId, Name = CurriculumName, Description = CurriculumDescription };
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course1.Id });
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course2.Id });
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course3.Id });

            var curriculum2 = new Curriculum { Id = CurriculumValidId2, Name = CurriculumName2, Description = CurriculumDescription2 };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3, course4);
            await db.Curriculums.AddRangeAsync(curriculum2, curriculum1);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCurriculumsWithCertificatesAndDiplomas()
        {
            var course1 = new Course { Id = CourseValidId, Name = CourseName2, StartDate = DateTime1 };
            var course2 = new Course { Id = CourseValidId2, Name = CourseName1, StartDate = DateTime1 };
            var course3 = new Course { Id = CourseValidId3, Name = CourseName1, StartDate = DateTime2 };

            var curriculum1 = new Curriculum { Id = CurriculumValidId };
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course1.Id });
            curriculum1.Courses.Add(new CurriculumCourse { CourseId = course2.Id });

            var curriculum2 = new Curriculum { Id = CurriculumValidId2 };
            curriculum2.Courses.Add(new CurriculumCourse { CourseId = course2.Id });
            curriculum2.Courses.Add(new CurriculumCourse { CourseId = course3.Id });

            // Not Candicate for Curriculum1 => Existing Diploma
            var student1 = new User { Id = "1111", UserName = "username1", Name = StudentNameC, Email = "email1@gmail.com" };
            var certificate1 = new Certificate { Id = "certificate1", StudentId = student1.Id, CourseId = course1.Id, };
            var certificate2 = new Certificate { Id = "certificate2", StudentId = student1.Id, CourseId = course2.Id, };
            var diploma1 = new Diploma { Id = "diploma1", CurriculumId = curriculum1.Id, StudentId = student1.Id };

            // Not Candidate for Curriculum1 => missing certificate 1
            var student2 = new User { Id = "2222", UserName = "username2", Name = StudentNameC, Email = "email2@gmail.com" };
            var certificate3 = new Certificate { Id = "certificate3", StudentId = student2.Id, CourseId = course2.Id, };
            var certificate4 = new Certificate { Id = "certificate4", StudentId = student2.Id, CourseId = course3.Id, };
            var diploma2 = new Diploma { Id = "diploma2", CurriculumId = curriculum2.Id, StudentId = student2.Id };

            // Student 3 & 4 are candidates for Curriculum1 => all certificates & no diplomas
            var student3 = new User { Id = "3333", UserName = "UsernameBBB", Name = StudentNameB, Email = "email3@gmail.com" };
            var certificate5 = new Certificate { Id = "certificate5", StudentId = student3.Id, CourseId = course1.Id, };
            var certificate6 = new Certificate { Id = "certificate6", StudentId = student3.Id, CourseId = course2.Id, };

            var student4 = new User { Id = "4444", UserName = "UsernameAAA", Name = StudentNameA, Email = "email4@gmail.com" };
            var certificate7 = new Certificate { Id = "certificate7", StudentId = student4.Id, CourseId = course1.Id, };
            var certificate8 = new Certificate { Id = "certificate8", StudentId = student4.Id, CourseId = course2.Id, };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3);
            await db.Curriculums.AddRangeAsync(curriculum1, curriculum2);
            await db.Certificates.AddRangeAsync(certificate1, certificate2, certificate3, certificate4, certificate5, certificate6, certificate7, certificate8);
            await db.Diplomas.AddRangeAsync(diploma1, diploma2);
            await db.Users.AddRangeAsync(student1, student2, student3, student4);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCurriculumsWithDiplomas()
        {
            var student1 = new User { Id = "1111", UserName = "username1", Name = StudentNameC, Email = "email1@gmail.com" };
            var student2 = new User { Id = "2222", UserName = "username2", Name = StudentNameB, Email = "email2@gmail.com" };
            var student3 = new User { Id = "3333", UserName = "username3", Name = StudentNameA, Email = "email3@gmail.com" };

            var curriculum1 = new Curriculum { Id = CurriculumValidId };
            var curriculum2 = new Curriculum { Id = CurriculumValidId2 };

            var diploma1 = new Diploma { Id = "AAA", CurriculumId = curriculum1.Id, StudentId = student1.Id, IssueDate = new DateTime(2019, 8, 1) };
            var diploma2 = new Diploma { Id = "BBB", CurriculumId = curriculum1.Id, StudentId = student2.Id, IssueDate = new DateTime(2019, 8, 11) };
            var diploma3 = new Diploma { Id = "CCC", CurriculumId = curriculum1.Id, StudentId = student3.Id, IssueDate = new DateTime(2019, 8, 21) };

            var diploma4 = new Diploma { Id = "DDD", CurriculumId = curriculum2.Id, StudentId = student1.Id, IssueDate = new DateTime(2019, 8, 21) };

            var db = Tests.InitializeDatabase();
            await db.Curriculums.AddRangeAsync(curriculum2, curriculum1);
            await db.Diplomas.AddRangeAsync(diploma1, diploma2, diploma3, diploma4);
            await db.Users.AddRangeAsync(student1, student2, student3);
            await db.SaveChangesAsync();

            return db;
        }

        private IAdminCurriculumService InitializeCurriculumService(UniversityDbContext db)
            => new AdminCurriculumService(db, Tests.Mapper);
    }
}
