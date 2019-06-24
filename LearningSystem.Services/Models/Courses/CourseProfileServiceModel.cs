namespace LearningSystem.Services.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseProfileServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [IgnoreMap]
        public Grade? Grade { get; set; }
    }
}
