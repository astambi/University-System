namespace University.Services.Models.Resources
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ResourceDetailsServiceModel : ResourceServiceModel, IMapFrom<Resource>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
