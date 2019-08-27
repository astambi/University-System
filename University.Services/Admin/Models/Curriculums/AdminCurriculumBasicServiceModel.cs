namespace University.Services.Admin.Models.Curriculums
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class AdminCurriculumBasicServiceModel : IMapFrom<Curriculum>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
