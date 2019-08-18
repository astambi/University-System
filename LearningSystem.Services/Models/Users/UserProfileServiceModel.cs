namespace LearningSystem.Services.Models.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class UserProfileServiceModel : UserServiceModel, IMapFrom<User>, IHaveCustomMapping
    {
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        public int CoursesCount { get; set; }

        public int CertificatesCount { get; set; }

        public int OrdersCount { get; set; }

        public int ResourcesCount { get; set; }

        public int ExamSubmissionsCount { get; set; }

        public int ArticlesCount { get; set; }

        public void ConfigureMapping(Profile mapper)
            => mapper.CreateMap<User, UserProfileServiceModel>()
            .ForMember(dest => dest.ResourcesCount, opt
                => opt.MapFrom(u =>
                u.Courses
                .Select(sc => sc.Course)
                .Sum(c => c.Resources.Count)));
    }
}
