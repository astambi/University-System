namespace LearningSystem.Services.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseProfileServiceModel : IMapFrom<StudentCourse>, IHaveCustomMapping
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        [DataType(DataType.Date)]
        public DateTime CourseStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime CourseEndDate { get; set; }

        public Grade? Grade { get; set; }

        public string CertificateId { get; set; }

        public void ConfigureMapping(Profile mapper)
            => mapper
            .CreateMap<StudentCourse, CourseProfileServiceModel>()
            .ForMember(
                dest => dest.CertificateId,
                opt => opt.MapFrom(src => src
                .Course
                .Certificates
                .Where(c => c.StudentId == src.StudentId)
                .OrderBy(c => c.Grade)
                .Select(c => c.Id)
                .FirstOrDefault()));
    }
}
