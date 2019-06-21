namespace LearningSystem.Web.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Helpers;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        private readonly ICourseService courseService;

        public HomeController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            var pageCourses = await this.courseService.AllWithTrainers(currentPage, WebConstants.PageSize);
            var totalCourses = await this.courseService.TotalAsync();

            var totalPages = PaginationHelpers.GetTotalPages(totalCourses, WebConstants.PageSize);
            currentPage = PaginationHelpers.GetValidCurrentPage(currentPage, totalPages);

            var model = new CoursePageListingViewModel
            {
                Courses = pageCourses,
                Pagination = new PaginationModel
                {
                    Action = nameof(Index),
                    CurrentPage = currentPage,
                    TotalPages = totalPages
                }
            };

            return this.View(model);
        }

        public IActionResult Privacy()
            => this.View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}
