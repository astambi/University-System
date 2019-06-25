namespace LearningSystem.Services.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string TrainerId { get; set; }

        [IgnoreMap]
        public string TrainerName { get; set; }

        public int Duration
            => this.EndDate.AddDays(1).Subtract(this.StartDate).Days;

        public TimeSpan RemainingTimeTillStart // in local time
            => this.StartDate.ToLocalTime().Subtract(DateTime.Now);

        public bool CanEnroll
            => this.RemainingTimeTillStart.Ticks > 0;
    }
}
