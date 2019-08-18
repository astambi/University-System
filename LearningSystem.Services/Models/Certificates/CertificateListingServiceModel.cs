namespace LearningSystem.Services.Models.Certificates
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CertificateListingServiceModel : IMapFrom<Certificate>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; }

        public Grade Grade { get; set; }        
    }
}
