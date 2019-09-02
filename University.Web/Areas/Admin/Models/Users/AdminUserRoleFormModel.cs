namespace University.Web.Areas.Admin.Models.Users
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Web.Models;

    public class AdminUserRoleFormModel
    {
        [Required]
        [HiddenInput]
        public string UserId { get; set; }

        public string Username { get; set; } // friendly delete confirmation dialog

        [Required]
        public string Role { get; set; } // friendly delete confirmation dialog

        public IEnumerable<SelectListItem> Roles { get; set; }

        [HiddenInput]
        public FormActionEnum Action { get; set; }
    }
}
