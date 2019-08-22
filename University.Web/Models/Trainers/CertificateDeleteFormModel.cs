namespace University.Web.Models.Trainers
{
    using System.ComponentModel.DataAnnotations;

    public class CertificateDeleteFormModel
    {
        [Required]
        public string CertificateId { get; set; }

        public int CourseId { get; set; }
    }
}
