﻿namespace LearningSystem.Services.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Common.Infrastructure.Extensions;
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

        public decimal Price { get; set; }

        public string TrainerId { get; set; }

        public string TrainerName { get; set; }

        public int Duration
            => this.StartDate.DaysTo(this.EndDate);

        public TimeSpan RemainingTimeTillStart // in local time
            => this.StartDate.RemainingTimeTillStart();

        public bool CanEnroll
            => !this.StartDate.HasEnded();
    }
}
