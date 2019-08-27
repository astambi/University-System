namespace University.Services.Admin.Models.Courses
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class AdminCourseBasicServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }
    }
}
