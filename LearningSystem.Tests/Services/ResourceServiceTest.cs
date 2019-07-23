namespace LearningSystem.Tests.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
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
        public async Task CreateAsync_ShouldNotSaveInDb_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            // Act
            // Invalid Course
            var result1 = await resourseService.CreateAsync(CourseInvalid, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>());

            // Invalid FileName
            var result2 = await resourseService.CreateAsync(CourseValid, "  ", It.IsAny<string>(), It.IsAny<byte[]>());
            var result3 = await resourseService.CreateAsync(CourseValid, null, It.IsAny<string>(), It.IsAny<byte[]>());

            // Invalid ContentType
            var result4 = await resourseService.CreateAsync(CourseValid, It.IsAny<string>(), "  ", It.IsAny<byte[]>());
            var result5 = await resourseService.CreateAsync(CourseValid, It.IsAny<string>(), null, It.IsAny<byte[]>());

            var resultCount = db.Resources.Count();

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.False(result4);
            Assert.False(result5);

            Assert.Equal(0, resultCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();

            await db.Courses.AddAsync(new Course { Id = CourseValid });
            await db.SaveChangesAsync();

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
        public async Task RemoveAsync_ShouldNotChangeDb_GivenInvalidInput()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            var testResource = new Resource { Id = ResourceValid };
            await db.Resources.AddAsync(testResource);
            await db.SaveChangesAsync();

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
            var db = Tests.InitializeDatabase();
            var resourseService = this.InitializeResourceService(db);

            var testResource = new Resource { Id = ResourceValid };
            await db.Resources.AddAsync(testResource);
            await db.SaveChangesAsync();

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

        private IResourceService InitializeResourceService(LearningSystemDbContext db)
            => new ResourceService(db, Tests.Mapper);
    }
}
