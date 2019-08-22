﻿namespace University.Services.Models.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using University.Common.Mapping;
    using University.Data.Models;
    using University.Services.Models.Certificates;

    public class StudentInCourseServiceModel : IMapFrom<StudentCourse>, IHaveCustomMapping
    {
        public string StudentId { get; set; }

        public string StudentUserName { get; set; }

        public string StudentEmail { get; set; }

        public string StudentName { get; set; }

        public decimal? GradeBg { get; set; }

        public bool HasExamSubmission { get; set; }

        public IEnumerable<CertificateListingServiceModel> Certificates { get; set; }

        public void ConfigureMapping(Profile mapper)
            => mapper
            .CreateMap<StudentCourse, StudentInCourseServiceModel>()
            .ForMember(
                dest => dest.HasExamSubmission,
                opt => opt.MapFrom(src => src
                    .Student
                    .ExamSubmissions
                    .Any(e => e.CourseId == src.CourseId)))
            .ForMember(
                dest => dest.Certificates,
                opt => opt.MapFrom(src => src
                    .Student
                    .Certificates
                    .Where(c => c.CourseId == src.CourseId)
                    .OrderByDescending(c => c.GradeBg)));
    }
}