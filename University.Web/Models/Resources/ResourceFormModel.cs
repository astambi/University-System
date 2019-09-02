namespace University.Web.Models.Resources
{
    public class ResourceFormModel
    {
        public int ResourceId { get; set; }

        public int CourseId { get; set; }

        public string ResourceName { get; set; } // friendly delete confirmation dialog
    }
}
