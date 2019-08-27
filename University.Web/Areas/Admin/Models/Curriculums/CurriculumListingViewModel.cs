namespace University.Web.Areas.Admin.Models.Curriculums
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Services.Admin.Models.Curriculums;

    public class CurriculumListingViewModel
    {
        public IEnumerable<AdminCurriculumServiceModel> Curriculums { get; set; }

        public IEnumerable<SelectListItem> CoursesSelectList { get; set; }
    }
}
