namespace University.Services.Models.Users
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserBasicServiceModel : IMapFrom<User>
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
