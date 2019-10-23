namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Certificates;
    using University.Services.Models.Courses;
    using University.Services.Models.Exams;
    using University.Services.Models.Resources;
    using University.Services.Models.Users;
    using Xunit;

    public class UserServiceTest
    {
        private const int CourseIdFirst = 10;
        private const int CourseIdSecond = 2;

        private const string CourseNameFirst = "AAAAA";
        private const string CourseNameSecond = "BBBBB";

        private const int CurriculumIdFirst = 1;
        private const int CurriculumIdSecond = 2;

        private const string CurriculumNameFirst = "AAAAA";
        private const string CurriculumNameSecond = "BBBBB";

        private const string FileNameFirst = "AAAAA";
        private const string FileNameSecond = "BBBBB";
        private const string FileNameThird = "CCCCC";

        private const string UserIdValid = "UserValid";
        private const string UserIdInvalid = "UserInvalid";

        private const string UserEmail = "user@gmail.com";
        private const string UserName = "User Name";
        private const string UserNameEdit = "     User Name     ";
        private const string UserUsername = "Username";

        private readonly DateTime UserBirthDate = new DateTime(1990, 3, 15);
        private readonly DateTime UserBirthDateToEdit = new DateTime(1990, 3, 25);

        private readonly DateTime DateFirst = new DateTime(2019, 7, 5);
        private readonly DateTime DateSecond = new DateTime(2019, 7, 15);
        private readonly DateTime DateThird = new DateTime(2019, 7, 25);

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenUserHasArticles()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await db.Articles.AddAsync(new Article { AuthorId = UserIdValid });
            await db.SaveChangesAsync();

            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenUserIsCourseTrainer()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await db.Courses.AddAsync(new Course { TrainerId = UserIdValid });
            await db.SaveChangesAsync();

            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnTrue_GivenUserIsNotCourseTrainerAndHasNoArticle()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.CanBeDeletedAsync(UserIdValid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetProfileToEditAsync_ShouldReturnNull_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileToEditAsync(UserIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileToEditAsync_ShouldReturnCorrectData_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileToEditAsync(UserIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserEditServiceModel>(result);

            Assert.Equal(UserName, result.Name);
            Assert.Equal(this.UserBirthDate, result.Birthdate);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturCorrectData_GivenValidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileAsync(UserIdInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnNull_GivenInvalidUser()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetProfileAsync(UserIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserProfileServiceModel>(result);

            Assert.Equal(UserIdValid, result.Id);
            Assert.Equal(UserEmail, result.Email);
            Assert.Equal(UserUsername, result.Username);
            Assert.Equal(UserName, result.Name);
            Assert.Equal(this.UserBirthDate, result.Birthdate);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldNotUpdate_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var resultInvalidUser = await userService.UpdateProfileAsync(
                UserIdInvalid,
                It.IsAny<string>(),
                DateTime.Now);

            var resultInvalidName = await userService.UpdateProfileAsync(
                UserIdValid,
                name: "          ",
                DateTime.Now);

            // Assert
            Assert.False(resultInvalidUser);
            Assert.False(resultInvalidName);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUser();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.UpdateProfileAsync(
                UserIdValid, UserNameEdit, this.UserBirthDateToEdit);

            var resultUser = await db.Users.FindAsync(UserIdValid);

            // Assert
            Assert.True(result);
            Assert.Equal(UserNameEdit.Trim(), resultUser.Name);
            Assert.Equal(this.UserBirthDateToEdit, resultUser.Birthdate);
        }

        [Fact]
        public async Task GetCoursesAsync_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetCoursesAsync(UserIdInvalid);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCoursesAsync_ShouldReturnCorrectData_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserCourses();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetCoursesAsync(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseProfileMaxGradeServiceModel>>(result);
            Assert.NotEmpty(result);

            var resultList = result.ToList();
            for (var i = 0; i < resultList.Count; i++)
            {
                var resultItem = resultList[i];
                var expected = db.Courses
                    .Where(c => c.Id == resultItem.CourseId)
                    .Select(c => new
                    {
                        Course = c,
                        Grade = c.Students
                            .Where(sc => sc.StudentId == UserIdValid)
                            .Select(sc => sc.GradeBg)
                            .FirstOrDefault(),
                        CertificateId = c.Certificates
                            .Where(cert => cert.StudentId == UserIdValid)
                            .OrderByDescending(cert => cert.GradeBg)
                            .Select(cert => cert.Id)
                            .FirstOrDefault(),
                        CertificateGrade = c.Certificates
                            .Where(cert => cert.StudentId == UserIdValid)
                            .OrderByDescending(cert => cert.GradeBg)
                            .Select(cert => cert.GradeBg)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                var expectedCourse = expected.Course;
                Assert.Equal(expectedCourse.Id, resultItem.CourseId);
                Assert.Equal(expectedCourse.Name, resultItem.CourseName);
                Assert.Equal(expectedCourse.StartDate, resultItem.CourseStartDate);
                Assert.Equal(expectedCourse.EndDate, resultItem.CourseEndDate);

                var maxGrade = expected.CertificateGrade != 0 ? expected.CertificateGrade : expected.Grade;

                Assert.Equal(maxGrade, resultItem.GradeBgMax);
                Assert.Equal(expected.CertificateId, resultItem.CertificateId);
            }

            // Assert Collection Sorting
            var resultIdsSorting = result.Select(c => c.CourseId).ToList();
            var expectedIdsSorting = db.Courses
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .Select(c => c.Id)
                .ToList();

            Assert.Equal(expectedIdsSorting, resultIdsSorting);
        }

        [Fact]
        public void GetCertificates_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetCertificates(UserIdInvalid);

            // Assert
            Assert.Empty(result);
            Assert.IsAssignableFrom<IEnumerable<CertificatesByCourseServiceModel>>(result);
        }

        [Fact]
        public async Task GetCertificates_ShouldReturnCorrectDataAndOrder_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserCertificates();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetCertificates(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CertificatesByCourseServiceModel>>(result);

            // Assert Group Order by Course name ASC
            var resultCourseGroups = result
                .Select(r => new CourseModel { CourseId = r.CourseId, CourseName = r.CourseName })
                .ToList();
            var courseFirst = db.Courses.Where(c => c.Name == CourseNameFirst).FirstOrDefault();
            var courseSecound = db.Courses.Where(c => c.Name == CourseNameSecond).FirstOrDefault();
            this.AssertCourseGroups(new[] { courseFirst, courseSecound }, resultCourseGroups);

            // Assert Certificates by Course group ordered by IssueDate DESC
            var resultGroup1 = result.First();
            var resultGroup2 = result.Last();
            var certificateFirst = db.Certificates.Where(c => c.IssueDate == this.DateFirst).FirstOrDefault();
            var certificateSecond = db.Certificates.Where(c => c.IssueDate == this.DateSecond).FirstOrDefault();
            var certificateThird = db.Certificates.Where(c => c.IssueDate == this.DateThird).FirstOrDefault();

            this.AssertCertificatesInGroup(new[] { certificateThird }, resultGroup1.Certificates);
            this.AssertCertificatesInGroup(new[] { certificateSecond, certificateFirst }, resultGroup2.Certificates);
        }

        [Fact]
        public async Task GetDiplomasAsync_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetDiplomasAsync(UserIdInvalid);

            // Assert
            Assert.Empty(result);
            Assert.IsAssignableFrom<IEnumerable<UserDiplomaListingServiceModel>>(result);
        }

        [Fact]
        public async Task GetDiplomasAsync_ShouldReturnCorrectDataAndOrder_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserDiplomas();
            var userService = this.InitializeUserService(db);

            // Act
            var result = await userService.GetDiplomasAsync(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<UserDiplomaListingServiceModel>>(result);

            // Assert Order by Curriculum name ASC
            Assert.Equal(new[] { CurriculumNameFirst, CurriculumNameSecond }, result.Select(d => d.CurriculumName).ToList());

            var resultList = result.ToList();
            foreach (var actual in result)
            {
                var expected = db.Diplomas.Find(actual.Id);
                this.AssertDiplomaDetails(expected, actual);
            }
        }

        [Fact]
        public void GetExams_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetExams(UserIdInvalid);

            // Assert
            Assert.Empty(result);
            Assert.IsAssignableFrom<IEnumerable<ExamsByCourseServiceModel>>(result);
        }

        [Fact]
        public async Task GetExams_ShouldReturnCorrectDataAndOrder_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserExams();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetExams(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ExamsByCourseServiceModel>>(result);

            // Assert Group Order by Course name ASC
            var resultCourseGroups = result
                .Select(r => new CourseModel { CourseId = r.CourseId, CourseName = r.CourseName })
                .ToList();
            var courseFirst = db.Courses.Where(c => c.Name == CourseNameFirst).FirstOrDefault();
            var courseSecound = db.Courses.Where(c => c.Name == CourseNameSecond).FirstOrDefault();
            this.AssertCourseGroups(new[] { courseFirst, courseSecound }, resultCourseGroups);

            // Assert Exams by Course group ordered by Submission Date DESC
            var resultGroup1 = result.First();
            var resultGroup2 = result.Last();
            var examFirst = db.ExamSubmissions.Where(c => c.SubmissionDate == this.DateFirst).FirstOrDefault();
            var examSecond = db.ExamSubmissions.Where(c => c.SubmissionDate == this.DateSecond).FirstOrDefault();
            var examThird = db.ExamSubmissions.Where(c => c.SubmissionDate == this.DateThird).FirstOrDefault();

            this.AssertExamsInGroup(new[] { examThird }, resultGroup1.Exams);
            this.AssertExamsInGroup(new[] { examSecond, examFirst }, resultGroup2.Exams);
        }

        [Fact]
        public void GetResources_ShouldReturnEmptyCollection_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetResources(UserIdInvalid);

            // Assert
            Assert.Empty(result);
            Assert.IsAssignableFrom<IEnumerable<ResourcesByCourseServiceModel>>(result);
        }

        [Fact]
        public async Task GetResources_ShouldReturnCorrectDataAndOrder_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareUserResources();
            var userService = this.InitializeUserService(db);

            // Act
            var result = userService.GetResources(UserIdValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ResourcesByCourseServiceModel>>(result);

            // Assert Group Order by Course name ASC
            var resultCourseGroups = result
                .Select(r => new CourseModel { CourseId = r.CourseId, CourseName = r.CourseName })
                .ToList();
            var courseFirst = db.Courses.Where(c => c.Name == CourseNameFirst).FirstOrDefault();
            var courseSecound = db.Courses.Where(c => c.Name == CourseNameSecond).FirstOrDefault();
            this.AssertCourseGroups(new[] { courseFirst, courseSecound }, resultCourseGroups);

            // Assert Resources by Course group ordered by Filename ASC
            var resultGroup1 = result.First();
            var resultGroup2 = result.Last();
            var resourceFirst = db.Resources.Where(c => c.FileName == FileNameFirst).FirstOrDefault();
            var resourceSecond = db.Resources.Where(c => c.FileName == FileNameSecond).FirstOrDefault();
            var resourceThird = db.Resources.Where(c => c.FileName == FileNameThird).FirstOrDefault();

            this.AssertResourcesInGroup(new[] { resourceFirst }, resultGroup1.Resources);
            this.AssertResourcesInGroup(new[] { resourceSecond, resourceThird }, resultGroup2.Resources);
        }

        private void AssertCourseGroups(IList<Course> expectedGroups, IEnumerable<CourseModel> actualGroups)
        {
            var actualList = actualGroups.ToList();
            Assert.Equal(expectedGroups.Count, actualList.Count);

            for (var i = 0; i < expectedGroups.Count; i++)
            {
                var expectedCourse = expectedGroups[i];
                var actualCourse = actualList[i];

                Assert.Equal(expectedCourse.Id, actualCourse.CourseId);
                Assert.Equal(expectedCourse.Name, actualCourse.CourseName);
            }
        }

        private void AssertCertificatesInGroup(IList<Certificate> expectedGroup, IEnumerable<CertificateListingServiceModel> actualGroup)
        {
            var actualGroupList = actualGroup.ToList();
            Assert.Equal(expectedGroup.Count, actualGroupList.Count);

            for (var i = 0; i < expectedGroup.Count; i++)
            {
                var expectedCertificate = expectedGroup[i];
                var actualCertificate = actualGroupList[i];

                Assert.Equal(expectedCertificate.Id, actualCertificate.Id);
                Assert.Equal(expectedCertificate.GradeBg, actualCertificate.GradeBg);
                Assert.Equal(expectedCertificate.IssueDate, actualCertificate.IssueDate);
            }
        }

        private void AssertDiplomaDetails(Diploma expected, UserDiplomaListingServiceModel actual)
        {
            Assert.Equal(expected.IssueDate, actual.IssueDate);
            Assert.Equal(expected.CurriculumId, actual.CurriculumId);
            Assert.Equal(expected.Curriculum.Name, actual.CurriculumName);
        }

        private void AssertExamsInGroup(IList<ExamSubmission> expectedGroup, IEnumerable<ExamSubmissionServiceModel> actualGroup)
        {
            var actualGroupList = actualGroup.ToList();
            Assert.Equal(expectedGroup.Count, actualGroupList.Count);

            for (var i = 0; i < expectedGroup.Count; i++)
            {
                var expectedExam = expectedGroup[i];
                var actualExam = actualGroupList[i];

                Assert.Equal(expectedExam.Id, actualExam.Id);
                Assert.Equal(expectedExam.SubmissionDate, actualExam.SubmissionDate);
            }
        }

        private void AssertResourcesInGroup(IList<Resource> expectedGroup, IEnumerable<ResourceServiceModel> actualGroup)
        {
            var actualGroupList = actualGroup.ToList();
            Assert.Equal(expectedGroup.Count, actualGroupList.Count);

            for (var i = 0; i < expectedGroup.Count; i++)
            {
                var expectedResource = expectedGroup[i];
                var actualResource = actualGroupList[i];

                Assert.Equal(expectedResource.Id, actualResource.Id);
                Assert.Equal(expectedResource.FileName, actualResource.FileName);
            }
        }

        private async Task<UniversityDbContext> PrepareUser()
        {
            var db = Tests.InitializeDatabase();
            await db.Users.AddAsync(new User
            {
                Id = UserIdValid,
                UserName = UserUsername,
                Email = UserEmail,
                Name = UserName,
                Birthdate = UserBirthDate
            });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserCourses()
        {
            var course1 = new Course { Id = 1, Name = "Course 1", StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2019, 3, 10) }; // third
            var course2 = new Course { Id = 2, Name = "Course 1", StartDate = new DateTime(2019, 7, 10), EndDate = new DateTime(2019, 7, 20) }; // second
            var course3 = new Course { Id = 3, Name = "Course 1", StartDate = new DateTime(2019, 7, 10), EndDate = new DateTime(2019, 8, 10) }; // first

            var user = new User { Id = UserIdValid };
            user.Courses.Add(new StudentCourse { CourseId = course1.Id, GradeBg = DataConstants.GradeBgMaxValue });
            user.Courses.Add(new StudentCourse { CourseId = course2.Id, GradeBg = DataConstants.GradeBgMinValue });
            user.Courses.Add(new StudentCourse { CourseId = course3.Id, GradeBg = null });

            var certificate1 = new Certificate { Id = "1", CourseId = course1.Id, StudentId = UserIdValid, GradeBg = DataConstants.GradeBgMaxValue };
            var certificate2 = new Certificate { Id = "2", CourseId = course2.Id, StudentId = UserIdValid, GradeBg = DataConstants.GradeBgCertificateMinValue };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3);
            await db.Users.AddAsync(user);
            await db.Certificates.AddRangeAsync(certificate1, certificate2);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserCertificates()
        {
            var user = new User { Id = UserIdValid };
            var userOther = new User { Id = "AnotherUser" };

            var course1 = new Course { Id = CourseIdSecond, Name = CourseNameSecond };
            var course2 = new Course { Id = CourseIdFirst, Name = CourseNameFirst };

            var certificate1 = new Certificate { Id = "CertificateAAA", CourseId = CourseIdSecond, GradeBg = DataConstants.GradeBgCertificateMinValue, IssueDate = DateFirst, StudentId = UserIdValid };
            var certificate2 = new Certificate { Id = "CertificateBBB", CourseId = CourseIdSecond, GradeBg = DataConstants.GradeBgMaxValue, IssueDate = DateSecond, StudentId = UserIdValid };
            var certificate3 = new Certificate { Id = "CertificateCCC", CourseId = CourseIdFirst, GradeBg = DataConstants.GradeBgMaxValue, IssueDate = DateThird, StudentId = UserIdValid };

            var certificateOther = new Certificate { Id = "CertificateDDD", CourseId = CourseIdSecond, GradeBg = DataConstants.GradeBgMaxValue, IssueDate = DateThird, StudentId = UserIdInvalid };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2);
            await db.Certificates.AddRangeAsync(certificate1, certificate2, certificate3, certificateOther);
            await db.Users.AddRangeAsync(user, userOther);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserDiplomas()
        {
            var studentWithDiploma = new User { Id = UserIdValid };
            var userOther = new User { Id = "AnotherUser" };

            var curriculum1 = new Curriculum { Id = CurriculumIdFirst, Name = CurriculumNameSecond };
            var curriculum2 = new Curriculum { Id = CurriculumIdSecond, Name = CurriculumNameFirst };

            var diploma1 = new Diploma { Id = "DiplomaAAA", IssueDate = DateFirst, CurriculumId = CurriculumIdFirst, StudentId = studentWithDiploma.Id };
            var diploma2 = new Diploma { Id = "DiplomaBBB", IssueDate = DateSecond, CurriculumId = CurriculumIdSecond, StudentId = studentWithDiploma.Id };
            var diplomaOther = new Diploma { Id = "DiplomaDDD", IssueDate = DateThird, CurriculumId = CurriculumIdFirst, StudentId = UserIdInvalid };

            var db = Tests.InitializeDatabase();
            await db.Curriculums.AddRangeAsync(curriculum1, curriculum2);
            await db.Diplomas.AddRangeAsync(diploma1, diploma2, diplomaOther);
            await db.Users.AddRangeAsync(studentWithDiploma, userOther);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserExams()
        {
            var user = new User { Id = UserIdValid };
            var userOther = new User { Id = "AnotherUser" };

            var course1 = new Course { Id = CourseIdSecond, Name = CourseNameSecond };
            var course2 = new Course { Id = CourseIdFirst, Name = CourseNameFirst };

            var exam1 = new ExamSubmission { Id = 1, CourseId = CourseIdSecond, SubmissionDate = DateFirst, StudentId = UserIdValid };
            var exam2 = new ExamSubmission { Id = 2, CourseId = CourseIdSecond, SubmissionDate = DateSecond, StudentId = UserIdValid };
            var exam3 = new ExamSubmission { Id = 3, CourseId = CourseIdFirst, SubmissionDate = DateThird, StudentId = UserIdValid };

            var examOther = new ExamSubmission { Id = 4, CourseId = CourseIdSecond, SubmissionDate = DateThird, StudentId = UserIdInvalid };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2);
            await db.ExamSubmissions.AddRangeAsync(exam1, exam2, exam3, examOther);
            await db.Users.AddRangeAsync(user, userOther);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserResources()
        {
            var user = new User { Id = UserIdValid };
            var userOther = new User { Id = "AnotherUser" };

            var course1 = new Course { Id = CourseIdSecond, Name = CourseNameSecond };
            var course2 = new Course { Id = CourseIdFirst, Name = CourseNameFirst };
            user.Courses.Add(new StudentCourse { CourseId = CourseIdFirst });
            user.Courses.Add(new StudentCourse { CourseId = CourseIdSecond });

            var courseOther = new Course { Id = 3, Name = "Some other course" };

            var resource1 = new Resource { Id = 1, CourseId = CourseIdSecond, FileName = FileNameThird };
            var resource2 = new Resource { Id = 2, CourseId = CourseIdSecond, FileName = FileNameSecond };
            var resource3 = new Resource { Id = 3, CourseId = CourseIdFirst, FileName = FileNameFirst };

            var resourceOther = new Resource { Id = 4, CourseId = courseOther.Id, FileName = "Some other course resource" };

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, courseOther);
            await db.Resources.AddRangeAsync(resource1, resource2, resource3, resourceOther);
            await db.Users.AddRangeAsync(user, userOther);
            await db.SaveChangesAsync();

            return db;
        }

        private IUserService InitializeUserService(UniversityDbContext db)
            => new UserService(db, Tests.Mapper);

        private class CourseModel
        {
            public int CourseId { get; set; }

            public string CourseName { get; set; }
        }
    }
}
