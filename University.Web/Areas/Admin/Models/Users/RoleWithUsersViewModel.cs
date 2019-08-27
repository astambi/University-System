namespace University.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using University.Services.Admin.Models.Users;

    public class RoleWithUsersViewModel
    {
        public string Role { get; set; }

        public IEnumerable<AdminUserListingServiceModel> UsersInRole { get; set; }
    }
}
