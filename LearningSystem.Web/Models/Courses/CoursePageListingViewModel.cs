namespace LearningSystem.Web.Models.Courses
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Courses;

    public class CoursePageListingViewModel
    {
        public IEnumerable<CourseServiceModel> Courses { get; set; }

        public PaginationModel Pagination { get; set; }

        public SearchModel Search { get; set; }
    }
}
