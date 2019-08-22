namespace University.Tests.Mocks
{
    using University.Data.Models;
    using University.Services;
    using University.Services.Models.Exams;
    using Moq;

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

        public static Mock<IExamService> DownloadForTrainerAsync(this Mock<IExamService> mock, ExamDownloadServiceModel exam)
        {
            mock
                .Setup(s => s.DownloadForTrainerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(exam)
                .Verifiable();

            return mock;
        }
    }
}
