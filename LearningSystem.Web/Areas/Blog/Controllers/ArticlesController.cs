namespace LearningSystem.Web.Areas.Blog.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Blog;
    using LearningSystem.Web.Areas.Blog.Models.Articles;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Infrastructure.Filters;
    using LearningSystem.Web.Infrastructure.Helpers;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Area(WebConstants.BlogArea)]
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IArticleService articleService;

        public ArticlesController(
            UserManager<User> userManager,
            IArticleService articleService)
        {
            this.userManager = userManager;
            this.articleService = articleService;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            var pageArticles = await this.articleService.AllAsync(currentPage, WebConstants.PageSize);
            var totalArticles = await this.articleService.TotalAsync();

            var totalPages = PaginationHelpers.GetTotalPages(totalArticles, WebConstants.PageSize);
            currentPage = PaginationHelpers.GetValidCurrentPage(currentPage, totalPages);

            var model = new ArticlePageListingViewModel
            {
                Articles = pageArticles,
                Pagination = new PaginationModel
                {
                    Action = nameof(Index),
                    CurrentPage = currentPage,
                    TotalPages = totalPages
                }
            };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.BlogAuthorRole)]
        public IActionResult Create() => this.View();

        [Authorize(Roles = WebConstants.BlogAuthorRole)]
        [HttpPost]
        [ValidateModelState] // attribute simple model validation
        public async Task<IActionResult> Create(ArticleFormModel model)
        {
            //if (!this.ModelState.IsValid)
            //{
            //    return this.View(model);
            //}

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.View(model);
            }

            // Raw Html Content sanitized in service
            await this.articleService.CreateAsync(model.Title, model.Content, userId);

            this.TempData.AddSuccessMessage(WebConstants.ArticlePublishedMsg);

            return this.RedirectToAction(nameof(Index));
        }

        
    }
}