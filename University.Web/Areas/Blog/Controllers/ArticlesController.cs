namespace University.Web.Areas.Blog.Controllers
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services.Blog;
    using University.Web.Areas.Blog.Models.Articles;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Infrastructure.Filters;
    using University.Web.Models;

    [Area(WebConstants.BlogArea)]
    [Authorize]
    public class ArticlesController : Controller
    {
        private const string ArticleFormView = "ArticleForm";
        private const string ArticleFriendlyUrl =
            WebConstants.BlogArea + "/" + WebConstants.ArticlesController + "/" + WebConstants.WithId + "/" + WebConstants.WithOptionalTitle; // SEO friendly URL

        private readonly UserManager<User> userManager;
        private readonly IArticleService articleService;
        private readonly IMapper mapper;

        public ArticlesController(
            UserManager<User> userManager,
            IArticleService articleService,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.articleService = articleService;
            this.mapper = mapper;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchTerm, int currentPage = 1)
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = searchTerm,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.articleService.TotalAsync(searchTerm)
            };

            var search = new SearchViewModel
            {
                Controller = WebConstants.ArticlesController,
                Action = nameof(Index),
                SearchTerm = searchTerm,
                Placeholder = WebConstants.SearchByArticleTitleOrContent
            };

            var articles = await this.articleService.AllAsync(searchTerm, pagination.CurrentPage, WebConstants.PageSize);

            var model = new ArticlePageListingViewModel
            {
                Articles = articles,
                Pagination = pagination,
                Search = search
            };

            return this.View(model);
        }

        // SEO friendly URL
        [Route(ArticleFriendlyUrl)]
        public async Task<IActionResult> Details(int id)
        {
            var model = await this.articleService.GetByIdAsync(id);
            if (model == null)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.BloggerRole)]
        public IActionResult Create()
            => this.View(ArticleFormView, new ArticleFormModel { Action = FormActionEnum.Create });

        [Authorize(Roles = WebConstants.BloggerRole)]
        [HttpPost]
        [ValidateModelState(ArticleFormView)] // attribute simple model validation replaces: if (!this.ModelState.IsValid) etc.
        public async Task<IActionResult> Create(ArticleFormModel model)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.View(model);
            }

            // Raw Html Content sanitized in service
            var id = await this.articleService.CreateAsync(model.Title, model.Content, userId);
            if (id < 0)
            {

                this.TempData.AddErrorMessage(WebConstants.ArticleCreateErrorMsg);
                return this.View(model);
            }

            this.TempData.AddSuccessMessage(WebConstants.ArticleCreateSuccessMsg);
            return this.RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = WebConstants.BloggerRole)]
        public async Task<IActionResult> Edit(int id)
            => await this.LoadArticleForm(id, FormActionEnum.Edit);

        [Authorize(Roles = WebConstants.BloggerRole)]
        [HttpPost]
        [ValidateModelState(ArticleFormView)] // attribute simple model validation replaces: if (!this.ModelState.IsValid) etc.
        public async Task<IActionResult> Edit(int id, ArticleFormModel model)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var exists = await this.articleService.ExistsForAuthorAsync(id, userId);
            if (!exists
                || id != model.Id)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleNotFoundForAuthorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var success = await this.articleService.UpdateAsync(id, model.Title, model.Content, userId);
            if (!success)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleUpdateErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.ArticleUpdateSuccessMsg);
            }

            return this.RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = WebConstants.BloggerRole)]
        public async Task<IActionResult> Delete(int id)
            => await this.LoadArticleForm(id, FormActionEnum.Delete);

        [Authorize(Roles = WebConstants.BloggerRole)]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, ArticleFormModel model)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var exists = await this.articleService.ExistsForAuthorAsync(id, userId);
            if (!exists
                || id != model.Id)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleNotFoundForAuthorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var success = await this.articleService.RemoveAsync(id, userId);
            if (!success)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleDeleteErrorMsg);
                return this.View(ArticleFormView, model);
            }

            this.TempData.AddSuccessMessage(WebConstants.ArticleDeleteSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> LoadArticleForm(int id, FormActionEnum action)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var exists = await this.articleService.ExistsAsync(id);
            if (!exists)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var article = await this.articleService.GetByIdToEditAsync(id, userId);
            if (article == null)
            {
                this.TempData.AddInfoMessage(WebConstants.ArticleNotFoundForAuthorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = this.mapper.Map<ArticleFormModel>(article);
            model.Action = action;

            return this.View(ArticleFormView, model);
        }
    }
}