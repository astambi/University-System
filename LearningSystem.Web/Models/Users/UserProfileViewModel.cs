namespace LearningSystem.Web.Models.Users
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;

    public class UserProfileViewModel
    {
        public UserWithBirthdateServiceModel User { get; set; }

        public IEnumerable<CourseProfileServiceModel> Courses { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
