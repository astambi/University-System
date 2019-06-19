namespace LearningSystem.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class AdminUserRoleFormModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }

        public FormAction Action { get; set; }
    }
}
