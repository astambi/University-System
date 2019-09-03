namespace University.Tests.Mocks
{
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using Moq;

    public static class CloudinaryMock
    {
        public static Mock<Cloudinary> GetMock
            => new Mock<Cloudinary>(Mock.Of<Account>());

        public static Mock<Cloudinary> Upload(this Mock<Cloudinary> mock, RawUploadResult result)
        {
            mock
                .Setup(s => s.Upload(It.IsAny<RawUploadParams>(), null))
                .Returns(result)
                .Verifiable();

            return mock;
        }
    }
}
