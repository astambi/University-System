namespace University.Web.Areas.Admin.Models.Users
{
    using System.ComponentModel.DataAnnotations;
    using University.Data;
    using University.Web.Models;

    public class AdminRoleFormModel
    {
        [Required]
        [StringLength(DataConstants.RoleNameMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        [Display(Name = "Role")]
        public string Name { get; set; }

        public FormActionEnum Action { get; set; } = FormActionEnum.Create;
    }
}
