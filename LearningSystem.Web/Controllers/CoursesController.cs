namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Mvc;

    public class CoursesController : Controller
    {
        private readonly ICourseService courseService;

        public CoursesController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            var pagination = new PaginationModel
            {
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.courseService.TotalAsync(false)
            };

            var courses = await this.courseService.AllWithTrainers(false, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination
            };

            return this.View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await this.courseService.GetById(id);

            if (course == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(course);
        }
    }
}