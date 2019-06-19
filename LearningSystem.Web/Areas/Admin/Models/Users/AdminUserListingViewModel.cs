namespace LearningSystem.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using LearningSystem.Services.Admin.Models;

    public class AdminUserListingViewModel
    {
        public IEnumerable<AdminUserListingServiceModel> Users { get; set; }

        public AdminUserRoleFormModel RoleFormModel { get; set; }
    }
}
