namespace LearningSystem.Tests.Mocks
{
    using System.Collections.Generic;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Moq;

    public static class TrainerServiceMock
    {
        public static Mock<ITrainerService> GetMock
            => new Mock<ITrainerService>();

        public static Mock<ITrainerService> AddCertificateAsync(this Mock<ITrainerService> mock, bool result)
        {
            mock
                .Setup(s => s.AddCertificateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Grade>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

        public static Mock<ITrainerService> AssessStudentCoursePerformanceAsync(this Mock<ITrainerService> mock, bool result)
        {
            mock
                .Setup(s => s.AssessStudentCoursePerformanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Grade>()))
                .ReturnsAsync(result)
                .Verifiable();

            return mock;
        }

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

        public static Mock<ITrainerService> DownloadExam(this Mock<ITrainerService> mock, ExamDownloadServiceModel exam)
        {
            mock
                .Setup(s => s.DownloadExam(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(exam)
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
