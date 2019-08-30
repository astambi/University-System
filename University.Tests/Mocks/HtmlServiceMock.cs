namespace University.Tests.Mocks
{
    using Moq;
    using University.Services;

    public static class HtmlServiceMock
    {
        public static Mock<IHtmlService> GetMock
            => new Mock<IHtmlService>();

        public static Mock<IHtmlService> Sanitize(this Mock<IHtmlService> mock, string result)
        {
            mock
                .Setup(s => s.Sanitize(It.IsAny<string>()))
                .Returns(result)
                .Verifiable();

            return mock;
        }
    }
}
