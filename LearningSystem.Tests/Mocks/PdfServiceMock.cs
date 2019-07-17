﻿namespace LearningSystem.Tests.Mocks
{
    using LearningSystem.Services;
    using Moq;

    public static class PdfServiceMock
    {
        public static Mock<IPdfService> GetMock
            => new Mock<IPdfService>();

        public static Mock<IPdfService> ConvertToPdf(this Mock<IPdfService> mock, byte[] fileBytes)
        {
            mock.Setup(s => s.ConvertToPdf(It.IsAny<string>()))
                .Returns(fileBytes)
                .Verifiable();
            return mock;
        }
    }
}
