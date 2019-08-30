namespace University.Services.Blog.Models
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class ArticleEditServiceModel : IMapFrom<Article>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}
