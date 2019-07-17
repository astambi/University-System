namespace LearningSystem.Tests.Mocks
{
    using System;
    using LearningSystem.Services.Admin;
    using Moq;

    public static class AdminCourseServiceMock
    {
        public static Mock<IAdminCourseService> GetMock
            => new Mock<IAdminCourseService>();

        public static Mock<IAdminCourseService> CreateAsync(this Mock<IAdminCourseService> mock, int courseId)
        {
            mock.Setup(a => a.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(courseId)
                .Verifiable();

            return mock;
        }
    }
}
