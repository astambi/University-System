namespace University.Services.Admin.Models.Courses
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using University.Common.Mapping;
    using University.Data.Models;

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

        public decimal Price { get; set; }

        public int Duration
            => this.EndDate.AddDays(1).Subtract(this.StartDate).Days;
    }
}
