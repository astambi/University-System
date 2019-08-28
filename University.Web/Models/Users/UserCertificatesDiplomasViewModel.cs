namespace University.Web.Models.Users
{
    using System.Collections.Generic;
    using University.Services.Models.Certificates;
    using University.Services.Models.Users;

    public class UserCertificatesDiplomasViewModel
    {
        public IEnumerable<CertificatesByCourseServiceModel> Certificates { get; set; }

        public IEnumerable<UserDiplomaListingServiceModel> Diplomas { get; set; }
    }
}
