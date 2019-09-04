namespace University.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Resources;
    using Xunit;

    public class ResourceServiceTest
    {
        private const int CourseInvalid = -1;
        private const int CourseValid = 100;

        private const int ResourceInvalid = -1;
        private const int ResourceValid = 100;

        private const string StudentEnrolled = "StudentEnrolled";
        private const string StudentNotEnrolled = "StudentNotEnrolled";
        private const string TrainerInvalid = "TrainerInvalid";
        private const string TrainerValid = "TrainerValid";

        private const string FileName = "   Resource name  ";
        private const string FileUrl = "https://res.cloudinary.com/filename.pptx";

        [Fact]
        public async Task AllByCourseAsync_ShouldReturnEmptyList_GivenInvalidCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.AllByCourseAsync(CourseInvalid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ResourceServiceModel>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AllByCourseAsync_ShouldReturnCorrectData_GivenValidCourse()
        {
            // Arrange
            var db = await this.PrepareResourcesCollection();
            var resourseService = this.InitializeResourceService(db);

            var resourcesOrdered = db.Resources.OrderBy(r => r.FileName);

            // Act
            var result = await resourseService.AllByCourseAsync(CourseValid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ResourceServiceModel>>(result);

            this.AssertResourcesCollection(resourcesOrdered, result);
        }

        [Fact]
        public async Task CanBeDownloadedByUser_ShouldReturnFalse_GivenInvalidResource()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.CanBeDownloadedByUserAsync(ResourceInvalid, It.IsAny<string>());

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(TrainerInvalid, false)]
        [InlineData(StudentNotEnrolled, false)]
        [InlineData(TrainerValid, true)]
        [InlineData(StudentEnrolled, true)]
        public async Task CanBeDownloadedByUser_ShouldReturnCorrectResult_GivenValidResource(string testUser, bool expectedResult)
        {
            // Arrange
            var db = await this.PrepareResourceWithStudentAndTrainer();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.CanBeDownloadedByUserAsync(ResourceValid, testUser);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveInDb_GivenInvalidCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            // Invalid Course
            var result = await resourseService.CreateAsync(CourseInvalid, It.IsAny<string>(), It.IsAny<string>());
            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result);

            Assert.Equal(0, resultCount);
        }

        [Theory]
        [InlineData("  ", FileUrl)]
        [InlineData(null, FileUrl)]
        [InlineData(FileName, "  ")]
        [InlineData(FileName, null)]
        public async Task CreateAsync_ShouldNotSaveInDb_GivenInvalidFile(string fileName, string fileUrl)
        {
            // Arrange
            var db = await this.PrepareCourse();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.CreateAsync(CourseValid, fileName, fileUrl);

            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(0, resultCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCourse();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.CreateAsync(CourseValid, FileName, FileUrl);
            var resultCount = db.Resources.Count();
            var resultResource = await db.Resources.FirstOrDefaultAsync();

            // Assert
            Assert.True(result);

            Assert.Equal(1, resultCount);

            Assert.NotNull(resultResource);
            Assert.Equal(CourseValid, resultResource.CourseId);
            Assert.Equal(FileName.Trim(), resultResource.FileName);
            Assert.Equal(FileUrl, resultResource.FileUrl);
        }

        [Fact]
        public void Exists_ShouldReturnFalse_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourceService = this.InitializeResourceService(db);

            // Act
            var result = resourceService.Exists(ResourceInvalid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Exists_ShouldReturnTrue_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourceService = this.InitializeResourceService(db);

            // Act
            var result = resourceService.Exists(ResourceValid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_ShouldReturnNull_GivenInvalidResource()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourceService = this.InitializeResourceService(db);

            // Act
            var result = await resourceService.GetDownloadUrlAsync(ResourceInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDownloadUrlAsync_ShouldReturnCorrectData_GivenValidResource()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourceService = this.InitializeResourceService(db);

            // Act
            var result = await resourceService.GetDownloadUrlAsync(ResourceValid);

            // Assert
            Assert.Equal(FileUrl, result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldNotChangeDb_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourseService = this.InitializeResourceService(db);

            var countBefore = db.Resources.Count();

            // Act
            var result = await resourseService.RemoveAsync(ResourceInvalid);
            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(countBefore, resultCount);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveEntity_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourseService = this.InitializeResourceService(db);

            var countBefore = db.Resources.Count();

            // Act
            var result = await resourseService.RemoveAsync(ResourceValid);
            var resultCount = db.Resources.Count();
            var resultEntity = await db.Resources.FindAsync(ResourceValid);

            // Assert
            Assert.True(result);
            Assert.Null(resultEntity);
            Assert.Equal(1, countBefore - resultCount);
        }

        private void AssertResourcesCollection(
            IEnumerable<Resource> expectedCollection,
            IEnumerable<ResourceServiceModel> resultCollection)
        {
            var expectedList = expectedCollection.ToList();
            var resultList = resultCollection.ToList();

            Assert.Equal(2, expectedList.Count);

            for (var i = 0; i < expectedList.Count; i++)
            {
                var expected = expectedList[i];
                var actual = resultList[i];

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.FileName, actual.FileName);
            }
        }

        private async Task<UniversityDbContext> PrepareCourse()
        {
            var db = Tests.InitializeDatabase();

            await db.Courses.AddAsync(new Course { Id = CourseValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareResource()
        {
            var db = Tests.InitializeDatabase();

            var testResource = new Resource
            {
                Id = ResourceValid,
                CourseId = CourseValid,
                FileName = FileName,
                FileUrl = FileUrl
            };

            await db.Resources.AddAsync(testResource);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareResourceWithStudentAndTrainer()
        {
            var db = Tests.InitializeDatabase();

            var studentEnrolled = new User { Id = StudentEnrolled };
            var studentNotEnrolled = new User { Id = StudentNotEnrolled };
            var trainerValid = new User { Id = TrainerValid };
            var trainerInvalid = new User { Id = TrainerInvalid };

            var resource = new Resource { Id = ResourceValid, CourseId = CourseValid };

            var course = new Course { Id = CourseValid, TrainerId = TrainerValid };

            course.Resources.Add(resource);
            course.Students.Add(new StudentCourse { StudentId = StudentEnrolled });

            await db.Users.AddRangeAsync(studentEnrolled, studentNotEnrolled, trainerValid, trainerInvalid);
            await db.Courses.AddAsync(course);
            await db.Resources.AddAsync(resource);

            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareResourcesCollection()
        {
            var db = Tests.InitializeDatabase();

            var testResource1 = new Resource
            {
                Id = 1,
                CourseId = CourseValid,
                FileName = "Resource B",
            };

            var testResource2 = new Resource
            {
                Id = 2,
                CourseId = CourseValid,
                FileName = "Resource A",
            };

            await db.Resources.AddRangeAsync(testResource1, testResource2);
            await db.SaveChangesAsync();

            return db;
        }

        private IResourceService InitializeResourceService(UniversityDbContext db)
            => new ResourceService(db, Tests.Mapper);
    }
}
