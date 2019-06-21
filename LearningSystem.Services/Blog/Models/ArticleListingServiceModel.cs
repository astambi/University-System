namespace LearningSystem.Services.Blog.Models
{
    using System;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class ArticleListingServiceModel : IMapFrom<Article>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime PublishDate { get; set; }
    }
}
