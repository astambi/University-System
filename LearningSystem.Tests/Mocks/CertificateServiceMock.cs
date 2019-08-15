﻿namespace LearningSystem.Tests.Mocks
{
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Certificates;
    using Moq;

    public static class CertificateServiceMock
    {
        public static Mock<ICertificateService> GetMock
            => new Mock<ICertificateService>();

        public static Mock<ICertificateService> CreateAsync(this Mock<ICertificateService> mock, bool result)
        {
            mock
                .Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Grade>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ICertificateService> DownloadAsync(this Mock<ICertificateService> mock, CertificateServiceModel certificate)
        {
            mock.Setup(s => s.DownloadAsync(It.IsAny<string>()))
                .ReturnsAsync(certificate)
                .Verifiable();
            return mock;
        }

        public static Mock<ICertificateService> IsGradeEligibleForCertificate(this Mock<ICertificateService> mock, bool result)
        {
            mock
                .Setup(s => s.IsGradeEligibleForCertificate(It.IsAny<Grade?>()))
                .Returns(result)
                .Verifiable();

            return mock;
        }
    }
}
