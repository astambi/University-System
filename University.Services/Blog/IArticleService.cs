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

        Task<int> CreateAsync(string title, string rawHtmlContent, string userId);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsForAuthorAsync(int articleId, string authorId);

        Task<ArticleDetailsServiceModel> GetByIdAsync(int id);

        Task<ArticleEditServiceModel> GetByIdToEditAsync(int id, string authorId);

        Task<bool> RemoveAsync(int id, string userId);

        Task<int> TotalAsync(string search = null);

        Task<bool> UpdateAsync(int id, string title, string rawHtmlContent, string userId);
    }
}
