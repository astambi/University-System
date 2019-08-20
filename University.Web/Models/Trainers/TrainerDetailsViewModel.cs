namespace University.Web.Models.Trainers
{
    using University.Services.Models.Users;
    using University.Web.Models.Courses;

    public class TrainerDetailsViewModel
    {
        public UserServiceModel Trainer { get; set; }

        public CoursePageListingViewModel Courses { get; set; }
    }
}
