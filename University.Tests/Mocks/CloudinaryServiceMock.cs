namespace University.Tests.Mocks
{
    using Moq;
    using University.Services;

    public static class CloudinaryServiceMock
    {
        public static Mock<ICloudinaryService> GetMock
            => new Mock<ICloudinaryService>();

        public static Mock<ICloudinaryService> UploadFile(this Mock<ICloudinaryService> mock, string result)
        {
            mock
                .Setup(s => s.UploadFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(result)
                .Verifiable();

            return mock;
        }
    }
}
