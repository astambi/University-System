namespace University.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Services.Admin.Models.Users;

    public class AdminUserListingViewModel
    {
        public IEnumerable<AdminUserListingServiceModel> Users { get; set; }

        public IEnumerable<RoleWithUsersViewModel> RolesWithUsersInRole { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}
