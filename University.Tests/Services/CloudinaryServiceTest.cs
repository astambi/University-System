namespace University.Tests.Services
{
    using University.Services.Implementations;
    using University.Tests.Mocks;
    using Xunit;

    public class CloudinaryServiceTest
    {
        private const string CloudFolder = "Folder";
        private const string FileName = "FileName.zip";

        private readonly byte[] FileBytes = new byte[] { 1, 2, 3 };

        [Fact]
        public void UploadFile_ShouldReturnNull_GivenInvalidFileBytes()
        {
            // Arrange
            var cloudinaryService = this.InitializeCloudinaryService();

            // Act
            var resultNull = cloudinaryService.UploadFile(null, FileName, CloudFolder);
            var resultEmptyArray = cloudinaryService.UploadFile(new byte[0], FileName, CloudFolder);

            // Assert
            Assert.Null(resultNull);
            Assert.Null(resultEmptyArray);
        }

        [Theory]
        [InlineData(null, CloudFolder)]
        [InlineData("  ", CloudFolder)]
        [InlineData(FileName, null)]
        [InlineData(FileName, "  ")]
        public void UploadFile_ShouldReturnNull_GivenInvalidInput(string fileName, string cloudFolder)
        {
            // Arrange
            var cloudinaryService = this.InitializeCloudinaryService();

            // Act
            var result = cloudinaryService.UploadFile(this.FileBytes, fileName, cloudFolder);

            // Assert
            Assert.Null(result);
        }

        private CloudinaryService InitializeCloudinaryService()
            => new CloudinaryService(CloudinaryMock.GetMock.Object);
    }
}
