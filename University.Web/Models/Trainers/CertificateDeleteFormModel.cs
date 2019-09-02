namespace University.Web.Models.Trainers
{
    using System.ComponentModel.DataAnnotations;

    public class CertificateDeleteFormModel
    {
        [Required]
        public string CertificateId { get; set; }

        public int CourseId { get; set; }

        public string StudentUsername { get; set; } // friendly delete confirmation dialog

        public decimal Grade { get; set; } // friendly delete confirmation dialog
    }
}
