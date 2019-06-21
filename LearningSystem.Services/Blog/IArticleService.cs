namespace LearningSystem.Services.Blog
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Services.Blog.Models;

    public interface IArticleService
    {
        Task<IEnumerable<ArticleWithAuthorListingServiceModel>> AllAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize);

        Task CreateAsync(string title, string rawHtmlContent, string userId);

        Task <ArticleDetailsWithAuthorServiceModel> GetByIdAsync(int id);

        Task<int> TotalAsync();
    }
}
