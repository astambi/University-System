namespace University.Tests.Mocks
{
    using Moq;
    using University.Services;

    public static class ExamServiceMock
    {
        public static Mock<IExamService> GetMock
            => new Mock<IExamService>();

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
