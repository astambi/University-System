namespace LearningSystem.Web.Models.Trainers
{
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Web.Models.Courses;

    public class TrainerDetailsViewModel
    {
        public UserServiceModel Trainer { get; set; }

        public CoursePageListingViewModel Courses { get; set; }
    }
}
