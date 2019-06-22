namespace LearningSystem.Services.Admin.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class AdminCourseServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TrainerId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public int Duration
            => this.EndDate.AddDays(1).Subtract(this.StartDate).Days;
    }
}
