namespace University.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using University.Services.Admin.Models;

    public class AdminUserListingViewModel
    {
        public IEnumerable<AdminUserListingServiceModel> Users { get; set; }

        public IEnumerable<RoleWithUsersViewModel> RolesWithUsersInRole { get; set; }

        public AdminUserRoleFormModel RoleFormModel { get; set; }
    }
}
