namespace University.Services.Models.Certificates
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class CertificateListingServiceModel : IMapFrom<Certificate>
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; }

        public Grade Grade { get; set; }        
    }
}
