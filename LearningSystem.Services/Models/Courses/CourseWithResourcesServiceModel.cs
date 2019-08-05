namespace LearningSystem.Services.Models.Courses
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseWithResourcesServiceModel : CourseServiceModel, IMapFrom<Course>, IHaveCustomMapping
    {
        public IEnumerable<CourseResourceServiceModel> Resources { get; set; }

        public void ConfigureMapping(/*IProfileExpression*/ Profile mapper)
            => mapper
            .CreateMap<Course, CourseWithResourcesServiceModel>()
            .ForMember(dest => dest.Resources,
                opt => opt.MapFrom(c => c.Resources
                .OrderBy(r => r.FileName)
                .Select(r => new CourseResourceServiceModel { Id = r.Id, FileName = r.FileName })
                .ToList()));
    }
}
