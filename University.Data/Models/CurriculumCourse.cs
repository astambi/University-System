namespace University.Data.Models
{
    public class CurriculumCourse
    {
        public int CurriculumId { get; set; }

        public Curriculum Curriculum { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }
    }
}
