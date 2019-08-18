namespace LearningSystem.Services.Models.Certificates
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CertificateDetailsListingServiceModel : CertificateListingServiceModel, IMapFrom<Certificate>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
