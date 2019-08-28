namespace University.Tests.Mocks
{
    using System.Collections.Generic;
    using Moq;
    using University.Services;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;

    public static class UserServiceMock
    {
        public static Mock<IUserService> GetMock
            => new Mock<IUserService>();

        public static Mock<IUserService> GetCoursesAsync(this Mock<IUserService> mock, IEnumerable<CourseProfileMaxGradeServiceModel> courses)
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
