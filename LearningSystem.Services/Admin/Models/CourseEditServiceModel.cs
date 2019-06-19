namespace LearningSystem.Services.Admin.Models
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CourseEditServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TrainerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
