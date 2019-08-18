using System;
using System.Collections.Generic;
using System.Text;

namespace LearningSystem.Services.Models.Certificates
{
    public class CertificatesByCourseServiceModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public IEnumerable<CertificateListingServiceModel> Certificates { get; set; }
    }
}
