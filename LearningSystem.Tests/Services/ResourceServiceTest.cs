namespace LearningSystem.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Resources;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;

    public class ResourceServiceTest
    {
        private const int CourseInvalid = -1;
        private const int CourseValid = 100;

        private const int ResourceInvalid = -1;
        private const int ResourceValid = 100;

        private const string FileName = "   Resource name  ";
        private const string ContentType = "ContentType";

        private readonly byte[] FileBytes = new byte[] { 100, 11, 127 };

        [Fact]
        public async Task AllByCourseAsync_ShouldReturnEmptyList_GivenInvalidCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.AllByCourseAsync(CourseInvalid);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CourseResourceServiceModel>>(result);
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
            Assert.IsAssignableFrom<IEnumerable<CourseResourceServiceModel>>(result);

            this.AssertResourcesCollection(resourcesOrdered, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveInDb_GivenInvalidCourse()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            // Invalid Course
            var result = await resourseService.CreateAsync(CourseInvalid, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>());
            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result);

            Assert.Equal(0, resultCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveInDb_GivenInvalidFile()
        {
            // Arrange
            var db = await this.PrepareCourse();
            var resourseService = this.InitializeResourceService(db);

            // Act
            // Invalid FileName
            var result1 = await resourseService.CreateAsync(CourseValid, "  ", It.IsAny<string>(), It.IsAny<byte[]>());
            var result2 = await resourseService.CreateAsync(CourseValid, null, It.IsAny<string>(), It.IsAny<byte[]>());

            // Invalid ContentType
            var result3 = await resourseService.CreateAsync(CourseValid, It.IsAny<string>(), "  ", It.IsAny<byte[]>());
            var result4 = await resourseService.CreateAsync(CourseValid, It.IsAny<string>(), null, It.IsAny<byte[]>());

            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);

            Assert.Equal(0, resultCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareCourse();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.CreateAsync(CourseValid, FileName, ContentType, this.FileBytes);
            var resultCount = db.Resources.Count();
            var resultResource = await db.Resources.FirstOrDefaultAsync();

            // Assert
            Assert.True(result);

            Assert.Equal(1, resultCount);

            Assert.NotNull(resultResource);
            Assert.Equal(CourseValid, resultResource.CourseId);
            Assert.Equal(FileName.Trim(), resultResource.FileName);
            Assert.Equal(ContentType, resultResource.ContentType);
            Assert.Equal(this.FileBytes, resultResource.FileBytes);
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.DownloadAsync(ResourceInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareResource();
            var resourseService = this.InitializeResourceService(db);

            // Act
            var result = await resourseService.DownloadAsync(ResourceValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ResourceDownloadServiceModel>(result);
            Assert.Equal(FileName, result.FileName);
            Assert.Equal(ContentType, result.ContentType);
            Assert.Equal(this.FileBytes, result.FileBytes);
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
            IEnumerable<CourseResourceServiceModel> resultCollection)
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

        private async Task<LearningSystemDbContext> PrepareCourse()
        {
            var db = Tests.InitializeDatabase();

            await db.Courses.AddAsync(new Course { Id = CourseValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<LearningSystemDbContext> PrepareResource()
        {
            var db = Tests.InitializeDatabase();

            var testResource = new Resource
            {
                Id = ResourceValid,
                CourseId = CourseValid,
                FileName = FileName,
                ContentType = ContentType,
                FileBytes = FileBytes,
            };

            await db.Resources.AddAsync(testResource);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<LearningSystemDbContext> PrepareResourcesCollection()
        {
            var db = Tests.InitializeDatabase();

            var testResource1 = new Resource
            {
                Id = 1,
                CourseId = CourseValid,
                FileName = "Resource B",
                ContentType = ContentType,
                FileBytes = FileBytes,
            };

            var testResource2 = new Resource
            {
                Id = 2,
                CourseId = CourseValid,
                FileName = "Resource A",
                ContentType = ContentType,
                FileBytes = FileBytes,
            };

            await db.Resources.AddRangeAsync(testResource1, testResource2);
            await db.SaveChangesAsync();

            return db;
        }

        private IResourceService InitializeResourceService(LearningSystemDbContext db)
            => new ResourceService(db, Tests.Mapper);
    }
}
