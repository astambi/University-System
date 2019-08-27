namespace University.Web.Areas.Admin.Models.Curriculums
{
    using System.ComponentModel.DataAnnotations;
    using University.Common.Mapping;
    using University.Data;
    using University.Services.Admin.Models.Curriculums;
    using University.Web.Models;

    public class CurriculumFormModel : IMapFrom<AdminCurriculumBasicServiceModel>
    {
        public int Id { get; set; } = int.MinValue; // Create

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
