namespace University.Tests.Mocks
{
    using Moq;
    using University.Services;

    public static class ResourceServiceMock
    {
        public static Mock<IResourceService> GetMock
            => new Mock<IResourceService>();

        public static Mock<IResourceService> CanBeDownloadedByUser(this Mock<IResourceService> mock, bool result)
        {
            mock
                .Setup(s => s.CanBeDownloadedByUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IResourceService> CreateAsync(this Mock<IResourceService> mock, bool result)
        {
            mock
                .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IResourceService> Exists(this Mock<IResourceService> mock, bool result)
        {
            mock
                .Setup(s => s.Exists(It.IsAny<int>()))
                .Returns(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IResourceService> GetDownloadUrlAsync(this Mock<IResourceService> mock, string result)
        {
            mock
                .Setup(s => s.GetDownloadUrlAsync(It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IResourceService> RemoveAsync(this Mock<IResourceService> mock, bool result)
        {
            mock
                .Setup(s => s.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }
    }
}
