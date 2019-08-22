namespace University.Services.Models.Certificates
{
    using System.Collections.Generic;

    public class CertificatesByCourseServiceModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public IEnumerable<CertificateListingServiceModel> Certificates { get; set; }
    }
}
