namespace University.Services.Models.Courses
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using University.Common.Mapping;
    using University.Data.Models;
    using University.Services.Models.Resources;

    public class CourseWithResourcesServiceModel : CourseServiceModel, IMapFrom<Course>, IHaveCustomMapping
    {
        public IEnumerable<ResourceServiceModel> Resources { get; set; }

        public void ConfigureMapping(IProfileExpression mapper)
            => mapper
            .CreateMap<Course, CourseWithResourcesServiceModel>()
            .ForMember(dest => dest.Resources,
                opt => opt.MapFrom(c => c.Resources
                .OrderBy(r => r.FileName)
                .Select(r => new ResourceServiceModel { Id = r.Id, FileName = r.FileName })
                .ToList()));
    }
}
