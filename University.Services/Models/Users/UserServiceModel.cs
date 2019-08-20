namespace University.Services.Models.Users
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserServiceModel : IMapFrom<User>
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
