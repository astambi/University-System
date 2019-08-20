namespace University.Services.Blog.Models
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ArticleDetailsServiceModel : ArticleListingServiceModel, IMapFrom<Article>
    {
        public string Content { get; set; }

        public string AuthorId { get; set; }

        public string AuthorUsername { get; set; }

        public string AuthorEmail { get; set; }
    }
}
