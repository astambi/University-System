namespace University.Services.Models.Certificates
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class CertificateDetailsListingServiceModel : CertificateListingServiceModel, IMapFrom<Certificate>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
