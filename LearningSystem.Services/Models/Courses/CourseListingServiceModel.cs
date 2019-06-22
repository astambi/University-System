namespace LearningSystem.Services.Models.Courses
{
    using LearningSystem.Services.Models.Users;

    public class CourseListingServiceModel
    {
        public CourseServiceModel Course { get; set; }

        public UserBasicServiceModel Trainer { get; set; }
    }
}