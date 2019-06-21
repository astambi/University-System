namespace LearningSystem.Services.Blog.Models
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models;

    public class ArticleDetailsWithAuthorServiceModel : IMapFrom<Article>
    {
        public ArticleDetailsServiceModel Article { get; set; }

        public UserServiceModel Author { get; set; }
    }
}
