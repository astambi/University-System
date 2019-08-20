namespace University.Services.Models.Resources
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ResourceDownloadServiceModel : IMapFrom<Resource>
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] FileBytes { get; set; }
    }
}
