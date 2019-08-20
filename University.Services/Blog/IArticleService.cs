namespace University.Services.Blog
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Blog.Models;

    public interface IArticleService
    {
        Task<IEnumerable<ArticleListingServiceModel>> AllAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task CreateAsync(string title, string rawHtmlContent, string userId);

        Task<ArticleDetailsServiceModel> GetByIdAsync(int id);

        Task<int> TotalAsync(string search = null);
    }
}
