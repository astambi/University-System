namespace LearningSystem.Services.Models.Resources
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ResourceDownloadServiceModel : IMapFrom<Resource>
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] FileBytes { get; set; }
    }
}
