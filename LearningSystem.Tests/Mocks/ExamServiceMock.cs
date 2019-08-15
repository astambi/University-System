namespace LearningSystem.Tests.Mocks
{
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Exams;
    using Moq;

    public static class ExamServiceMock
    {
        public static Mock<IExamService> GetMock
            => new Mock<IExamService>();

        public static Mock<IExamService> AssessAsync(this Mock<IExamService> mock, bool result)
        {
            mock
                .Setup(s => s.AssessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Grade>()))
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
