namespace University.Services.Models.Resources
{
    using System.Collections.Generic;

    public class ResourcesByCourseServiceModel
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public IEnumerable<ResourceServiceModel> Resources { get; set; }
    }
}
