namespace University.Services.Admin.Models.Users
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class AdminUserListingServiceModel : IMapFrom<User>
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}
