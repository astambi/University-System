namespace LearningSystem.Services.Models.Resources
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ResourceServiceModel : IMapFrom<Resource>
    {
        public int Id { get; set; }

        public string FileName { get; set; }
    }
}
