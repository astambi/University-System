namespace University.Web.Areas.Admin.Models.Curriculums
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CurriculumAddRemoveCourseFormModel
    {
        public int CurriculumId { get; set; }

        public int CourseId { get; set; }

        public IEnumerable<SelectListItem> Courses { get; set; }
    }
}
