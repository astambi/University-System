namespace University.Web.Areas.Admin.Models.Curriculums
{
    using System.Collections.Generic;
    using University.Services.Admin.Models.Curriculums;
    using University.Services.Admin.Models.Users;

    public class CurriculumGraduatesListingViewModel
    {
        public AdminCurriculumBasicServiceModel Curriculum { get; set; }

        public IEnumerable<AdminDiplomaGraduateServiceModel> Graduates { get; set; }

        public IEnumerable<AdminUserListingServiceModel> Candidates { get; set; }
    }
}
