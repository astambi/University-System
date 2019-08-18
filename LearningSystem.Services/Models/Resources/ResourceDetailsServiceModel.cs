namespace LearningSystem.Services.Models.Resources
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ResourceDetailsServiceModel : ResourceServiceModel, IMapFrom<Resource>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
