﻿namespace LearningSystem.Tests.Mocks
{
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Resources;
    using Moq;

    public static class ResourceServiceMock
    {
        public static Mock<IResourceService> GetMock
            => new Mock<IResourceService>();

        public static Mock<IResourceService> CreateAsync(this Mock<IResourceService> mock, bool result)
        {
            mock
                .Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IResourceService> DownloadAsync(this Mock<IResourceService> mock, ResourceDownloadServiceModel resource)
        {
            mock
                .Setup(s => s.DownloadAsync(It.IsAny<int>()))
                .ReturnsAsync(resource)
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