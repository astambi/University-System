namespace LearningSystem.Services.Models.Users
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class UserBasicServiceModel : IMapFrom<User>
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
