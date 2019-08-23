namespace University.Web.Areas.Admin.Models.Curriculums
{
    using System.ComponentModel.DataAnnotations;
    using University.Data;
    using University.Web.Models;

    public class CurriculumFormModel
    {
        [Required]
        [StringLength(DataConstants.CourseNameMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.CourseDescriptionMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Description { get; set; }

        public FormActionEnum Action { get; set; } = FormActionEnum.Create;
    }
}
