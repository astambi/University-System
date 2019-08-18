namespace LearningSystem.Tests.Mocks
{
    using System.Collections.Generic;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Moq;

    public static class UserServiceMock
    {
        public static Mock<IUserService> GetMock
            => new Mock<IUserService>();

        public static Mock<IUserService> GetCoursesAsync(this Mock<IUserService> mock, IEnumerable<CourseProfileServiceModel> courses)
        {
            mock.Setup(u => u.GetCoursesAsync(It.IsAny<string>()))
                .ReturnsAsync(courses)
                .Verifiable();
            return mock;
        }

        public static Mock<IUserService> GetProfileAsync(this Mock<IUserService> mock, UserProfileServiceModel userData)
        {
            mock.Setup(u => u.GetProfileAsync(It.IsAny<string>()))
                .ReturnsAsync(userData)
                .Verifiable();
            return mock;
        }
    }
}
