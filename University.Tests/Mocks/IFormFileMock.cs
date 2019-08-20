namespace University.Tests.Mocks
{
    using Microsoft.AspNetCore.Http;
    using Moq;

    public static class IFormFileMock
    {
        public static Mock<IFormFile> GetMock
            => new Mock<IFormFile>();
    }
}
