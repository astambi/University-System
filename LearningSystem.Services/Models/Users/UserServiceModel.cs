namespace LearningSystem.Services.Models.Users
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class UserServiceModel : IMapFrom<User>
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
