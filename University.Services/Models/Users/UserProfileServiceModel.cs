namespace University.Services.Models.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using AutoMapper;
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserProfileServiceModel : UserServiceModel, IMapFrom<User>, IHaveCustomMapping
    {
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        public int CoursesCount { get; set; }

        public int CertificatesCount { get; set; }

        public int DiplomasCount { get; set; }

        public int ExamSubmissionsCount { get; set; }

        public int ArticlesCount { get; set; }

        public int ResourcesCount { get; set; }

        public void ConfigureMapping(IProfileExpression mapper)
            => mapper
            .CreateMap<User, UserProfileServiceModel>()
            .ForMember(
                dest => dest.ResourcesCount,
                opt => opt.MapFrom(u => u
                    .Courses
                    .SelectMany(sc => sc.Course.Resources)
                    .Count()));
    }
}
