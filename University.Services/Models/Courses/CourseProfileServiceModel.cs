namespace University.Services.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using AutoMapper;
    using University.Common.Mapping;
    using University.Data.Models;

    public class CourseProfileServiceModel : IMapFrom<StudentCourse>, IHaveCustomMapping
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        [DataType(DataType.Date)]
        public DateTime CourseStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime CourseEndDate { get; set; }

        public decimal? GradeBg { get; set; }

        public string CertificateId { get; set; }

        public decimal? CertificateGrade { get; set; }

        public void ConfigureMapping(IProfileExpression mapper)
            => mapper
            .CreateMap<StudentCourse, CourseProfileServiceModel>()
            .ForMember(
                dest => dest.CertificateId,
                opt => opt.MapFrom(src => src
                .Course
                .Certificates
                .Where(c => c.StudentId == src.StudentId)
                .OrderByDescending(c => c.GradeBg)
                .Select(c => c.Id)
                .FirstOrDefault()))
            .ForMember(
                dest => dest.CertificateGrade,
                opt => opt.MapFrom(src => src
                .Course
                .Certificates
                .Where(c => c.StudentId == src.StudentId)
                .OrderByDescending(c => c.GradeBg)
                .Select(c => c.GradeBg)
                .FirstOrDefault())); // 0
    }
}
