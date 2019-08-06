namespace LearningSystem.Web.Models.Courses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Services.Models.Courses;

    public class CourseDetailsViewModel : IMapFrom<CourseDetailsServiceModel>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string TrainerId { get; set; }

        public string TrainerName { get; set; }

        public int Duration { get; set; }

        public TimeSpan RemainingTimeTillStart { get; set; }

        public bool CanEnroll { get; set; }

        public string Description { get; set; }

        public int StudentsCount { get; set; }

        public bool IsExamSubmissionDate { get; set; }

        [IgnoreMap]
        public bool IsUserEnrolled { get; set; }

        [IgnoreMap]
        public IEnumerable<CourseResourceServiceModel> Resources = new List<CourseResourceServiceModel>();
    }
}
