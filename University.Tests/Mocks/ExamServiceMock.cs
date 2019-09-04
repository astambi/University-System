namespace University.Tests.Mocks
{
    using Moq;
    using University.Services;

    public static class ExamServiceMock
    {
        public static Mock<IExamService> GetMock
            => new Mock<IExamService>();

        public static Mock<IExamService> CanBeDownloadedByUser(this Mock<IExamService> mock, bool result)
        {
            mock
                .Setup(s => s.CanBeDownloadedByUserAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IExamService> GetDownloadUrlAsync(this Mock<IExamService> mock, string result)
        {
            mock
                .Setup(s => s.GetDownloadUrlAsync(It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<IExamService> EvaluateAsync(this Mock<IExamService> mock, bool result)
        {
            mock
                .Setup(s => s.EvaluateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }
    }
}
