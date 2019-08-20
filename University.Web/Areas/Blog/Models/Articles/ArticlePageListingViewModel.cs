namespace University.Web.Areas.Blog.Models.Articles
{
    using System.Collections.Generic;
    using University.Services.Blog.Models;
    using University.Web.Models;

    public class ArticlePageListingViewModel
    {
        public IEnumerable<ArticleListingServiceModel> Articles { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public SearchViewModel Search { get; set; }
    }
}
