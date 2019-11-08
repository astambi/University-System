namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Certificates;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;
    using Xunit;

    public class TrainerServiceTest
    {
        private const string Certificate1 = "Certificate1";
        private const string Certificate2 = "Certificate2";
        private const string Certificate3 = "Certificate3";

        private const int CourseValid = 10;
        private const int CourseInvalid = 20;
        private const int CourseNotEnded = 30;
        private const int CourseEnded = 40;

        private const string SearchTerm = "T";

        private const string Student1 = "Student1";
        private const string Student2 = "Student2";
        private const string Student3 = "Student3";

        private const string TrainerValid = "TrainerValid";
        private const string TrainerInvalid = "TrainerInvalid";
        private const string TrainerInvalid2 = "TrainerInvalid2";

        private const string TrainerId = "TrainerId";
        private const string TrainerName = "Trainer name";
        private const string TrainerUsername = "TrainerUsername";
        private const string TrainerEmail = "email@gmail.com";

        private const int Precision = 100;

        private readonly DateTime Today = DateTime.UtcNow;

        [Fact]
        public async Task IsTrainerForCourseAsync_ShouldReturnFalse_GivenInvalidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var resultInvalidTrainer = await trainerService.IsTrainerForCourseAsync(TrainerInvalid, CourseValid);
            var resultInvalidCourse = await trainerService.IsTrainerForCourseAsync(TrainerValid, CourseInvalid);

            // Assert
            Assert.False(resultInvalidTrainer);
            Assert.False(resultInvalidCourse);
        }

        [Fact]
        public async Task IsTrainerForCourseAsync_ShouldReturnTrue_GivenValidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.IsTrainerForCourseAsync(TrainerValid, CourseValid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CourseHasEndedAsync_ShouldReturnFalse_GivenFutureCourseEndDate()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.CourseHasEndedAsync(CourseNotEnded);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CourseHasEndedAsync_ShouldReturnTrue_GivenPastCourseEndDate()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.CourseHasEndedAsync(CourseEnded);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CourseByIdAsync_ShouldReturnNull_GivenInvalidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var resultInvalidTrainer = await trainerService.CourseByIdAsync(TrainerInvalid, CourseValid);
            var resultInvalidCourse = await trainerService.CourseByIdAsync(TrainerValid, CourseInvalid);

            // Assert
            Assert.Null(resultInvalidTrainer);
            Assert.Null(resultInvalidCourse);
        }

        [Fact]
        public async Task CourseByIdAsync_ShouldReturnCorrectData_GivenValidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);
            var expected = this.GetTrainerCourse(db, TrainerValid, CourseValid);

            // Act
            var result = await trainerService.CourseByIdAsync(TrainerValid, CourseValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CourseServiceModel>(result);

            this.AssertCourseServiceModel(expected, result);
        }

        [Fact]
        public async Task CourseWithResourcesByIdAsync_ShouldReturnNull_GivenInvalidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var resultInvalidTrainer = await trainerService.CourseWithResourcesByIdAsync(TrainerInvalid, CourseValid);
            var resultInvalidCourse = await trainerService.CourseWithResourcesByIdAsync(TrainerValid, CourseInvalid);

            // Assert
            Assert.Null(resultInvalidTrainer);
            Assert.Null(resultInvalidCourse);
        }

        [Fact]
        public async Task CourseWithResourcesByIdAsync_ShouldReturnCorrectData_GivenValidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCoursesWithTrainers();
            var trainerService = this.InitializeTrainerService(db);
            var expected = this.GetTrainerCourse(db, TrainerValid, CourseValid);

            // Act
            var result = await trainerService.CourseWithResourcesByIdAsync(TrainerValid, CourseValid);
            var resultResources = result.Resources.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CourseWithResourcesServiceModel>(result);
            this.AssertCourseServiceModel(expected, result);

            var expectedResources = expected.Resources.ToList();

            Assert.Equal(expectedResources.Count, resultResources.Count);
            for (var i = 0; i < resultResources.Count; i++)
            {
                var expectedResource = expectedResources[i];
                var resultResource = resultResources[i];

                Assert.Equal(expectedResource.Id, resultResource.Id);
                Assert.Equal(expectedResource.FileName, resultResource.FileName);
            }
        }

        [Fact]
        public async Task StudentsInCourseAsync_ShouldReturnEmptyCollection_GivenInvalidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCourseStudentsWithCertificates();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.StudentsInCourseAsync(CourseInvalid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<StudentInCourseServiceModel>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task StudentsInCourseAsync_ShouldReturnCorrectData_GivenValidTrainerCourse()
        {
            // Arrange
            var db = await this.PrepareCourseStudentsWithCertificates();
            var trainerService = this.InitializeTrainerService(db);

            var expected = db
                .Users
                .Where(u => u.Courses.Any(sc => sc.CourseId == CourseValid))
                .ToList();

            // Act
            var result = await trainerService.StudentsInCourseAsync(CourseValid);
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<StudentInCourseServiceModel>>(result);

            Assert.Equal(new[] { Student3, Student2, Student1 }, resultList.Select(s => s.StudentId));

            this.AssertStudentsInCourse(expected, resultList);
            this.AssertHasExamSubmission(resultList);
            this.AssertGrade(resultList);
            this.AssertExamId(resultList);
        }

        [Fact]
        public async Task TotalCoursesAsync_ShouldReturnAllTrainerCourses_GivenNullSearchTerm()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToSearch();
            var trainerService = this.InitializeTrainerService(db);

            var expectedCount = db
                .Courses
                .Where(c => c.TrainerId == TrainerValid)
                .Count();

            // Act
            var result = await trainerService.TotalCoursesAsync(TrainerValid, null);

            // Assert
            Assert.Equal(expectedCount, result);
            Assert.Equal(7, result);
        }

        [Fact]
        public async Task TotalCoursesAsync_ShouldReturnCorrectCountByCourseNameCaseInsensitive_GivenSearchTerm()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToSearch();
            var trainerService = this.InitializeTrainerService(db);

            var expectedCount = db
                .Courses
                .Where(c => c.TrainerId == TrainerValid)
                .Where(c => c.Name.ToLower().Contains(SearchTerm.ToLower())) // case insensitive
                .Count();

            // Act
            var result = await trainerService.TotalCoursesAsync(TrainerValid, SearchTerm);

            // Assert
            Assert.Equal(expectedCount, result);
            Assert.Equal(6, result);
        }

        [Fact]
        public async Task CoursesAsync_ShouldReturnAllTrainerCoursesWithCorrectDataAndOrder_GivenNullSearchTerm()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToSearch();
            var trainerService = this.InitializeTrainerService(db);

            var expected = db
                .Courses
                .Where(c => c.TrainerId == TrainerValid)
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .ToList();

            // Act
            var result = await trainerService.CoursesAsync(TrainerValid, null);
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(result);
            Assert.Equal(expected.Count, result.Count());

            for (var i = 0; i < resultList.Count; i++)
            {
                var expectedItem = expected[i];
                var resultItem = resultList[i];

                this.AssertCourseServiceModel(expectedItem, resultItem);
            }
        }

        [Fact]
        public async Task CoursesAsync_ShouldReturnCorrectDataAndOrder_ByPageAndSearch()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToSearch();
            var trainerService = this.InitializeTrainerService(db);

            var expectedAll = db
                .Courses
                .Where(c => c.TrainerId == TrainerValid)
                .Where(c => c.Name.ToLower().Contains(SearchTerm.ToLower()))
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .Select(c => c.Id)
                .ToList();

            // Act
            var resultPageDefaultValues = await trainerService.CoursesAsync(TrainerValid, SearchTerm); // default 1 of 12
            var resultPage1of12 = await trainerService.CoursesAsync(TrainerValid, SearchTerm, 1, 12);

            var resultPage1of2 = await trainerService.CoursesAsync(TrainerValid, SearchTerm, 1, 2);
            var resultPage2of2 = await trainerService.CoursesAsync(TrainerValid, SearchTerm, 2, 2);

            var resultPagePositiveInvalid = await trainerService.CoursesAsync(TrainerValid, SearchTerm, 100, 12);

            var resultPageNegativeOf1 = await trainerService.CoursesAsync(TrainerValid, SearchTerm, -100, 1); // min 1 of 1
            var resultPageNegativeOfNegativeDefault = await trainerService.CoursesAsync(TrainerValid, SearchTerm, -100, -12); // min 1 of min 1

            // Assert
            Assert.Equal(expectedAll, resultPageDefaultValues.Select(c => c.Id).ToList());
            Assert.Equal(expectedAll, resultPage1of12.Select(c => c.Id).ToList());

            Assert.Equal(new List<int> { 3, 2 }, resultPage1of2.Select(c => c.Id).ToList());
            Assert.Equal(new List<int> { 1, 7 }, resultPage2of2.Select(c => c.Id).ToList());

            Assert.Empty(resultPagePositiveInvalid);

            Assert.Equal(new List<int> { 3 }, resultPageNegativeOf1.Select(c => c.Id).ToList());
            Assert.Equal(new List<int> { 3 }, resultPageNegativeOfNegativeDefault.Select(c => c.Id).ToList());
        }

        [Fact]
        public async Task CoursesToEvaluateAsync_ShouldReturnEmptyCollection_GivenInvalidTrainerOrCourseDates()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToEvaluate();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var resultInvalidTrainer = await trainerService.CoursesToEvaluateAsync(TrainerInvalid);
            var resultInvalidCourses = await trainerService.CoursesToEvaluateAsync(TrainerInvalid2);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(resultInvalidTrainer);
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(resultInvalidCourses);

            Assert.Empty(resultInvalidTrainer);
            Assert.Empty(resultInvalidCourses);
        }

        [Fact]
        public async Task CoursesToEvaluateAsync_ShouldReturnCorrectDataAndOrder_GivenValidTrainer()
        {
            // Arrange
            var db = await this.PrepareTrainerCoursesToEvaluate();
            var trainerService = this.InitializeTrainerService(db);

            var expected = db
                .Courses
                .Where(c => c.TrainerId == TrainerValid)
                .Where(c => c.EndDate < this.Today && this.Today <= c.EndDate.AddDays(ServicesConstants.EvaluationPeriodInDays))
                .OrderBy(c => c.Name)
                .ToList();

            // Act
            var result = await trainerService.CoursesToEvaluateAsync(TrainerValid);
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(result);

            Assert.Equal(new[] { 6, 5, 4, 3 }, result.Select(c => c.Id).ToList()); // correct order
            Assert.Equal(expected.Select(c => c.Id).ToList(), result.Select(c => c.Id).ToList()); // correct order

            Assert.Equal(expected.Count, result.Count());
            for (var i = 0; i < resultList.Count; i++)
            {
                var expectedItem = expected[i];
                var resultItem = resultList[i];

                this.AssertCourseServiceModel(expectedItem, resultItem);
            }
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnNull_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.GetProfileAsync(TrainerInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnCorrectData_GivenValidUser()
        {
            // Arrange
            var db = await this.PrepareTrainer();
            var trainerService = this.InitializeTrainerService(db);

            // Act
            var result = await trainerService.GetProfileAsync(TrainerId);

            // Assert
            Assert.IsType<UserServiceModel>(result);
            Assert.Equal(TrainerId, result.Id);
            Assert.Equal(TrainerName, result.Name);
            Assert.Equal(TrainerUsername, result.Username);
            Assert.Equal(TrainerEmail, result.Email);
        }

        private void AssertCourseServiceModel(Course expected, CourseServiceModel result)
        {
            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.StartDate, result.StartDate);
            Assert.Equal(expected.EndDate, result.EndDate);
            Assert.Equal(expected.Price, result.Price);
            Assert.Equal(expected.TrainerId, result.TrainerId);
            Assert.Equal(expected.Trainer.Name, result.TrainerName);
            Assert.Equal(expected.StartDate.DaysTo(expected.EndDate), result.Duration);
            Assert.Equal(!expected.StartDate.HasEnded(), result.CanEnroll);

            result.RemainingTimeTillStart
                .Should()
                .BeCloseTo(expected.StartDate.RemainingTimeTillStart(), Precision);
        }

        private void AssertExamId(List<StudentInCourseServiceModel> resultList)
        {
            Assert.Equal(0, resultList.First().ExamId);
            Assert.Equal(3, resultList.Skip(1).First().ExamId);
            Assert.Equal(2, resultList.Last().ExamId);
        }

        private void AssertGrade(List<StudentInCourseServiceModel> resultList)
        {
            Assert.Null(resultList.First().GradeBg);
            Assert.Equal(DataConstants.GradeBgCertificateMinValue, resultList.Skip(1).First().GradeBg);
            Assert.Equal(DataConstants.GradeBgMaxValue, resultList.Last().GradeBg);
        }

        private void AssertHasExamSubmission(List<StudentInCourseServiceModel> resultList)
        {
            Assert.False(resultList.First().HasExamSubmission);
            Assert.True(resultList.Skip(1).First().HasExamSubmission);
            Assert.True(resultList.Last().HasExamSubmission);
        }

        private void AssertStudentsInCourse(List<User> expected, List<StudentInCourseServiceModel> resultList)
        {
            Assert.Equal(expected.Count, resultList.Count);
            for (var i = 0; i < resultList.Count; i++)
            {
                var resultUser = resultList[i];
                var expectedUser = expected.FirstOrDefault(u => u.Id == resultUser.StudentId);

                this.AssertUser(expectedUser, resultUser);
                this.AssertCertificates(expectedUser.Certificates.ToList(), resultUser.Certificates.ToList());
            }
        }

        private void AssertCertificates(List<Certificate> expectedCertificates, List<CertificateListingServiceModel> resultCertificates)
        {
            Assert.Equal(expectedCertificates.Count, resultCertificates.Count);

            // Order
            Assert.Equal(
                expectedCertificates.OrderByDescending(c => c.GradeBg).Select(c => c.Id),
                resultCertificates.Select(c => c.Id));

            foreach (var result in resultCertificates)
            {
                var expected = expectedCertificates.FirstOrDefault(c => c.Id == result.Id);
                Assert.Equal(expected.GradeBg, result.GradeBg);
                Assert.Equal(expected.IssueDate, result.IssueDate);
            }
        }

        private void AssertUser(User expectedUser, StudentInCourseServiceModel resultUser)
        {
            Assert.Equal(expectedUser.Id, resultUser.StudentId);
            Assert.Equal(expectedUser.Name, resultUser.StudentName);
            Assert.Equal(expectedUser.UserName, resultUser.StudentUserName);
            Assert.Equal(expectedUser.Email, resultUser.StudentEmail);
        }

        private Course GetTrainerCourse(UniversityDbContext db, string trainerId, int courseId)
            => db.Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId)
            .FirstOrDefault();

        private async Task<UniversityDbContext> PrepareTrainer()
        {
            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(new User
            {
                Id = TrainerId,
                Name = TrainerName,
                UserName = TrainerUsername,
                Email = TrainerEmail
            });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareTrainerCoursesToEvaluate()
        {
            var endDate = this.Today.ToEndDateUtc();

            var trainer1 = new User { Id = TrainerValid };
            var trainer2 = new User { Id = TrainerInvalid2 };
            var trainer3 = new User { Id = TrainerInvalid };

            var courses = new List<Course>
            {
                new Course{Id = 2, Name = "YYY", TrainerId = TrainerValid, EndDate = endDate.AddDays(1) },  // active 
                new Course{Id = 1, Name = "XXX", TrainerId = TrainerValid, EndDate = endDate.AddDays(0) },  // to evaluate 

                new Course{Id = 3, Name = "RRR", TrainerId = TrainerValid, EndDate = endDate.AddDays(-1) },  // to evaluate 
                new Course{Id = 4, Name = "DDD", TrainerId = TrainerValid, EndDate = endDate.AddDays(-10) },  // to evaluate
                new Course{Id = 5, Name = "CCC", TrainerId = TrainerValid, EndDate = endDate.AddDays(-28) }, // to evaluate
                new Course{Id = 6, Name = "BBB", TrainerId = TrainerValid, EndDate = endDate.AddDays(1 - ServicesConstants.EvaluationPeriodInDays) }, // to evaluate

                //new Course{Id = 7, Name = "AAA", TrainerId = TrainerValid, EndDate = endDate.AddDays(0 - ServicesConstants.EvaluationPeriodInDays) }, // overdue
                new Course{Id = 8, Name = "AAA", TrainerId = TrainerInvalid2, EndDate = endDate.AddDays(-1 - ServicesConstants.EvaluationPeriodInDays) }, // overdue
                new Course{Id = 9, Name = "BBB", TrainerId = TrainerInvalid2, EndDate = endDate.AddDays(-35) }, // overdue
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(trainer1, trainer2, trainer3);
            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareTrainerCoursesToSearch()
        {
            var today = DateTime.Now;
            var startDate = today.ToStartDateUtc();
            var endDate = today.ToEndDateUtc();

            var trainer1 = new User { Id = TrainerValid };
            var trainer2 = new User { Id = TrainerInvalid };

            var courses = new List<Course>
            {
                new Course{Id = 1, Name = "TTT", TrainerId = TrainerValid, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(0) },  // active 2
                new Course{Id = 2, Name = "ttt", TrainerId = TrainerValid, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(1) },  // active 1
                new Course{Id = 3, Name = "Tt",  TrainerId = TrainerValid, StartDate = startDate.AddDays(1),  EndDate = endDate.AddDays(1) },  // active 0
                new Course{Id = 4, Name = "XXX", TrainerId = TrainerValid, StartDate = startDate.AddDays(0),  EndDate = endDate.AddDays(1) },  // no match
                new Course{Id = 5, Name = "TTT", TrainerId = TrainerValid, StartDate = startDate.AddDays(-2), EndDate = endDate.AddDays(-2) }, // archived 2 5
                new Course{Id = 6, Name = "ttt", TrainerId = TrainerValid, StartDate = startDate.AddDays(-2), EndDate = endDate.AddDays(-1) }, // archived 1 4
                new Course{Id = 7, Name = "Tt",  TrainerId = TrainerValid, StartDate = startDate.AddDays(-1), EndDate = endDate.AddDays(-1) }, // archived 0 3

                new Course{Id = 8, Name = "Tt",  TrainerId = TrainerInvalid, StartDate = startDate.AddDays(-1), EndDate = endDate.AddDays(-1) }, // archived 0
            };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(trainer1, trainer2);
            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCoursesWithTrainers()
        {
            var trainer1 = new User { Id = TrainerValid };
            var trainer2 = new User { Id = TrainerInvalid };

            var student1 = new User { Id = Student1, Name = "Student 1", UserName = "UsernameCCC", Email = "email1@gmail.com" };
            var student2 = new User { Id = Student2, Name = "Student 2", UserName = "UsernameBBB", Email = "email2@gmail.com" };
            var student3 = new User { Id = Student3, Name = "Student 3", UserName = "UsernameAAA", Email = "email3@gmail.com" };

            var course1 = new Course { Id = CourseValid, TrainerId = TrainerValid };
            var course2 = new Course { Id = CourseInvalid };
            var course3 = new Course { Id = CourseEnded, EndDate = DateTime.UtcNow.AddDays(-1) }; // past
            var course4 = new Course { Id = CourseNotEnded, EndDate = DateTime.UtcNow.AddDays(1) }; // future

            course1.Students.Add(new StudentCourse { StudentId = student1.Id, CourseId = CourseValid, GradeBg = DataConstants.GradeBgMaxValue });
            course1.Students.Add(new StudentCourse { StudentId = student2.Id, CourseId = CourseValid, GradeBg = DataConstants.GradeBgCertificateMinValue });
            course1.Students.Add(new StudentCourse { StudentId = student3.Id, CourseId = CourseValid });

            student1.ExamSubmissions.Add(new ExamSubmission { Id = 1, CourseId = CourseValid, SubmissionDate = new DateTime(2019, 8, 1) });
            student1.ExamSubmissions.Add(new ExamSubmission { Id = 2, CourseId = CourseValid, SubmissionDate = new DateTime(2019, 8, 10) });
            student2.ExamSubmissions.Add(new ExamSubmission { Id = 3, CourseId = CourseValid, SubmissionDate = new DateTime(2019, 8, 10) });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2, course3, course4);
            await db.Users.AddRangeAsync(trainer1, trainer2, student1, student2, student3);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareCourseStudentsWithCertificates()
        {
            var student1 = new User { Id = Student1, Name = "Student 1", UserName = "UsernameCCC", Email = "email1@gmail.com" };
            var student2 = new User { Id = Student2, Name = "Student 2", UserName = "UsernameBBB", Email = "email2@gmail.com" };
            var student3 = new User { Id = Student3, Name = "Student 3", UserName = "UsernameAAA", Email = "email3@gmail.com" };

            var course1 = new Course { Id = CourseValid };
            var course2 = new Course { Id = 2 };

            var examSubmissionDate1 = new DateTime(2019, 8, 1);
            var examSubmissionDate2 = new DateTime(2019, 10, 22);

            course1.Students.Add(new StudentCourse { StudentId = student1.Id, CourseId = course1.Id, GradeBg = DataConstants.GradeBgMaxValue });
            course1.Students.Add(new StudentCourse { StudentId = student2.Id, CourseId = course1.Id, GradeBg = DataConstants.GradeBgCertificateMinValue });
            course1.Students.Add(new StudentCourse { StudentId = student3.Id, CourseId = course1.Id });

            course2.Students.Add(new StudentCourse { StudentId = student3.Id, CourseId = course2.Id, GradeBg = DataConstants.GradeBgMaxValue });

            student1.ExamSubmissions.Add(new ExamSubmission { Id = 1, CourseId = course1.Id, SubmissionDate = examSubmissionDate1 });
            student1.ExamSubmissions.Add(new ExamSubmission { Id = 2, CourseId = course1.Id, SubmissionDate = examSubmissionDate2 });
            student2.ExamSubmissions.Add(new ExamSubmission { Id = 3, CourseId = course1.Id, SubmissionDate = examSubmissionDate2 });

            student1.Certificates.Add(new Certificate
            {
                Id = Certificate1,
                CourseId = course1.Id,
                StudentId = student1.Id,
                GradeBg = DataConstants.GradeBgMinValue,
                IssueDate = examSubmissionDate1
            });
            student1.Certificates.Add(new Certificate
            {
                Id = Certificate2,
                CourseId = course1.Id,
                StudentId = student1.Id,
                GradeBg = DataConstants.GradeBgMaxValue,
                IssueDate = examSubmissionDate2
            });
            student2.Certificates.Add(new Certificate
            {
                Id = Certificate3,
                CourseId = course1.Id,
                StudentId = student1.Id,
                GradeBg = DataConstants.GradeBgCertificateMinValue,
                IssueDate = examSubmissionDate2
            });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(course1, course2);
            await db.Users.AddRangeAsync(student1, student2, student3);
            await db.SaveChangesAsync();

            return db;
        }

        private ITrainerService InitializeTrainerService(UniversityDbContext db)
            => new TrainerService(db, Tests.Mapper);
    }
}
