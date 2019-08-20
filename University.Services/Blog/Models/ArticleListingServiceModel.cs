namespace University.Services.Blog.Models
{
    using System;
    using University.Common.Mapping;
    using University.Data.Models;

    public class ArticleListingServiceModel : IMapFrom<Article>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime PublishDate { get; set; }

        public string AuthorName { get; set; }
    }
}
