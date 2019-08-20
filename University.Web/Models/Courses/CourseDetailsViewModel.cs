namespace University.Web.Models.Courses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using University.Common.Mapping;
    using University.Services.Models.Courses;
    using University.Services.Models.Resources;

    public class CourseDetailsViewModel : IMapFrom<CourseDetailsServiceModel>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public string TrainerId { get; set; }

        public string TrainerName { get; set; }

        public string TrainerUsername { get; set; }

        public int Duration { get; set; }

        public TimeSpan RemainingTimeTillStart { get; set; }

        public bool CanEnroll { get; set; }

        public string Description { get; set; }

        public int StudentsCount { get; set; }

        public bool IsExamSubmissionDate { get; set; }

        [IgnoreMap]
        public bool IsTrainer { get; set; }

        [IgnoreMap]
        public bool IsUserEnrolled { get; set; }

        [IgnoreMap]
        public IEnumerable<ResourceServiceModel> Resources = new List<ResourceServiceModel>();
    }
}
