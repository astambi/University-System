namespace LearningSystem.Services.Models
{
    using LearningSystem.Services.Admin.Models;

    public class CourseListingServiceModel
    {
        public CourseServiceModel Course { get; set; }

        public UserServiceModel Trainer { get; set; }
    }
}