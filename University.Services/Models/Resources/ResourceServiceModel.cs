namespace University.Services.Models.Resources
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ResourceServiceModel : IMapFrom<Resource>
    {
        public int Id { get; set; }

        public string FileName { get; set; }
    }
}
