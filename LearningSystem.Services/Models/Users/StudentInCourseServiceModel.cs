namespace LearningSystem.Services.Models.Users
{
    using LearningSystem.Data.Models;

    public class StudentInCourseServiceModel
    {
        public UserServiceModel Student { get; set; }

        public Grade? Grade { get; set; }
    }
}
