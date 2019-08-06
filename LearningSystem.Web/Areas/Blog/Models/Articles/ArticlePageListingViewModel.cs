namespace LearningSystem.Web.Areas.Blog.Models.Articles
{
    using System.Collections.Generic;
    using LearningSystem.Services.Blog.Models;
    using LearningSystem.Web.Models;

    public class ArticlePageListingViewModel
    {
        public IEnumerable<ArticleListingServiceModel> Articles { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public SearchViewModel Search { get; set; }
    }
}
