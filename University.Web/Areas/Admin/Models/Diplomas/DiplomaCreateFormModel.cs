namespace University.Web.Areas.Admin.Models.Diplomas
{
    using System.ComponentModel.DataAnnotations;

    public class DiplomaCreateFormModel
    {
        [Required]
        public string StudentId { get; set; }

        public int CurriculumId { get; set; }
    }
}
