namespace University.Web.Areas.Admin.Models.Curriculums
{
    public class CurriculumCourseRemoveFormModel
    {
        public int CurriculumId { get; set; }

        public int CourseId { get; set; }

        public string CurriculumName { get; set; } // friendly delete confirmation dialog

        public string CourseName { get; set; } // friendly delete confirmation dialog
    }
}
