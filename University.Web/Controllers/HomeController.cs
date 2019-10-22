namespace University.Web.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using University.Services;
    using University.Web.Models;
    using University.Web.Models.Courses;

    public class HomeController : Controller
    {
        private readonly ICourseService courseService;

        public HomeController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        public async Task<IActionResult> Index(string searchTerm, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = searchTerm,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalActiveAsync(searchTerm)
            };

            var search = new SearchViewModel
            {
                Controller = WebConstants.CoursesController,
                Action = nameof(Index),
                SearchTerm = searchTerm,
                Placeholder = WebConstants.SearchByCourseName
            };

            var courses = await this.courseService.AllActiveAsync(searchTerm, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = search
            };

            return this.View(model);
        }

        public IActionResult Privacy()
            => this.View();

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
            => this.View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier
            });
    }
}
