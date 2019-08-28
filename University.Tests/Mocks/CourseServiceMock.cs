namespace University.Tests.Mocks
{
    using System.Collections.Generic;
    using Moq;
    using University.Services;
    using University.Services.Models.Courses;

    public static class CourseServiceMock
    {
        public static Mock<ICourseService> GetMock
            => new Mock<ICourseService>();

        public static Mock<ICourseService> AllActiveWithTrainersAsync(this Mock<ICourseService> mock, IEnumerable<CourseServiceModel> courses)
        {
            mock
                .Setup(s => s.AllActiveAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(courses)
                .Verifiable();

            return mock;
        }

        public static Mock<ICourseService> Exists(this Mock<ICourseService> mock, bool result)
        {
            mock
                .Setup(s => s.Exists(It.IsAny<int>()))
                .Returns(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ICourseService> GetByIdAsync(this Mock<ICourseService> mock, CourseDetailsServiceModel course)
        {
            mock
               .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
               .ReturnsAsync(course)
               .Verifiable();

            return mock;
        }

        public static Mock<ICourseService> IsUserEnrolledInCourseAsync(this Mock<ICourseService> mock, bool result)
        {
            mock.Setup(s => s.IsUserEnrolledInCourseAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ICourseService> TotalActiveAsync(this Mock<ICourseService> mock, int count)
        {
            mock
                .Setup(s => s.TotalActiveAsync(It.IsAny<string>()))
                .ReturnsAsync(count)
                .Verifiable();

            return mock;
        }
    }
}
