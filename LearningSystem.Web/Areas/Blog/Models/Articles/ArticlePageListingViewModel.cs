namespace LearningSystem.Web.Areas.Blog.Models.Articles
{
    using System.Collections.Generic;
    using LearningSystem.Services.Blog.Models;
    using LearningSystem.Web.Models;

    public class ArticlePageListingViewModel
    {
        public IEnumerable<ArticleWithAuthorListingServiceModel> Articles { get; set; }

        public PaginationModel Pagination { get; set; }

        public SearchModel Search { get; set; }
    }
}
