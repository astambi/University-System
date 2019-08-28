namespace University.Web.Areas.Admin.Models.Diplomas
{
    using System.ComponentModel.DataAnnotations;

    public class DiplomaDeleteFormModel
    {
        [Required]
        public string DiplomaId { get; set; }

        public int CurriculumId { get; set; }
    }
}
