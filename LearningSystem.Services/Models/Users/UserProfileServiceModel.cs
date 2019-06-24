namespace LearningSystem.Services.Models.Users
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Courses;

    public class UserProfileServiceModel
    {
        public UserWithBirthdateServiceModel User { get; set; }

        public IEnumerable<CourseProfileServiceModel> Courses { get; set; }
    }
}
