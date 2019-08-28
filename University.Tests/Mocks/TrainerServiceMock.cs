namespace University.Tests.Mocks
{
    using System.Collections.Generic;
    using Moq;
    using University.Services;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;

    public static class TrainerServiceMock
    {
        public static Mock<ITrainerService> GetMock
            => new Mock<ITrainerService>();

        public static Mock<ITrainerService> CourseByIdAsync(this Mock<ITrainerService> mock, CourseServiceModel course)
        {
            mock.Setup(s => s.CourseByIdAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(course)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> CourseHasEndedAsync(this Mock<ITrainerService> mock, bool result)
        {
            mock
                .Setup(s => s.CourseHasEndedAsync(It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> CoursesAsync(this Mock<ITrainerService> mock, IEnumerable<CourseServiceModel> courses)
        {
            mock
                .Setup(s => s.CoursesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(courses)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> CoursesToEvaluateAsync(this Mock<ITrainerService> mock, IEnumerable<CourseServiceModel> courses)
        {
            mock
                .Setup(s => s.CoursesToEvaluateAsync(It.IsAny<string>()))
                .ReturnsAsync(courses)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> IsTrainerForCourseAsync(this Mock<ITrainerService> mock, bool result)
        {
            mock
                .Setup(s => s.IsTrainerForCourseAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> StudentsInCourseAsync(this Mock<ITrainerService> mock, IEnumerable<StudentInCourseServiceModel> students)
        {
            mock.Setup(s => s.StudentsInCourseAsync(It.IsAny<int>()))
                .ReturnsAsync(students)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> TotalCoursesAsync(this Mock<ITrainerService> mock, int count)
        {
            mock
                .Setup(s => s.TotalCoursesAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(count)
                .Verifiable();

            return mock;
        }
    }
}
