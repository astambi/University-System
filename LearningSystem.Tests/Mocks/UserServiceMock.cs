namespace LearningSystem.Tests.Mocks
{
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Users;
    using Moq;

    public static class UserServiceMock
    {
        public static Mock<IUserService> GetMock
            => new Mock<IUserService>();

        public static Mock<IUserService> GetCertificateDataAsync(this Mock<IUserService> mock, CertificateServiceModel certificate)
        {
            mock.Setup(u => u.GetCertificateDataAsync(It.IsAny<string>()))
                .ReturnsAsync(certificate)
                .Verifiable();
            return mock;
        }

        public static Mock<IUserService> GetUserProfileAsync(this Mock<IUserService> mock, UserProfileServiceModel profile)
        {
            mock.Setup(u => u.GetUserProfileAsync(It.IsAny<string>()))
                .ReturnsAsync(profile)
                .Verifiable();
            return mock;
        }
    }
}
