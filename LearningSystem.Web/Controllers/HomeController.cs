namespace LearningSystem.Web.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using LearningSystem.Services;
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

        public async Task<IActionResult> Index(string search = null, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = search,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalActiveAsync(search)
            };

            var courses = await this.courseService.AllActiveAsync(search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = new SearchViewModel { SearchTerm = search, Placeholder = WebConstants.SearchByCourseName }
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
