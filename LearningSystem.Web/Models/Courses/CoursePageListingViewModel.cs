namespace LearningSystem.Web.Models.Courses
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models;

    public class CoursePageListingViewModel
    {
        public IEnumerable<CourseListingServiceModel> Courses { get; set; }

        public PaginationModel Pagination { get; set; }
    }
}
