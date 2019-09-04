namespace University.Tests.Mocks
{
    using CloudinaryDotNet;
    using Moq;

    public static class CloudinaryMock
    {
        public static Mock<Cloudinary> GetMock
            => new Mock<Cloudinary>(new Account("cloud", "apiKey", "apiSecret"));
    }
}
