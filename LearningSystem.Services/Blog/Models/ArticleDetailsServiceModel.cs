namespace LearningSystem.Services.Blog.Models
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ArticleDetailsServiceModel : ArticleListingServiceModel, IMapFrom<Article>
    {
        public string Content { get; set; }

        public string AuthorId { get; set; }

        public string AuthorUsername { get; set; }

        public string AuthorEmail { get; set; }
    }
}
