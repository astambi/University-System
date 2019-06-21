namespace LearningSystem.Services.Blog.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Blog.Models;
    using LearningSystem.Services.Html;
    using Microsoft.EntityFrameworkCore;

    public class ArticleService : IArticleService
    {
        private readonly LearningSystemDbContext db;
        private readonly IHtmlService htmlService;
        private readonly IMapper mapper;

        public ArticleService(
            LearningSystemDbContext db,
            IHtmlService htmlService,
            IMapper mapper)
        {
            this.db = db;
            this.htmlService = htmlService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ArticleWithAuthorListingServiceModel>> AllAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.db
            .Articles
            .OrderByDescending(a => a.PublishDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleWithAuthorListingServiceModel
            {
                Article = this.mapper.Map<ArticleListingServiceModel>(a),
                Author = a.Author.Name
            })
            .ToListAsync();

        public async Task CreateAsync(string title, string rawHtmlContent, string userId)
        {
            var userExists = this.db.Users.Any(u => u.Id == userId);
            if (!userExists)
            {
                return;
            }

            var sanitizedContent = this.htmlService.Sanitize(rawHtmlContent); // sanitized html

            var article = new Article
            {
                Title = title,
                Content = sanitizedContent,
                AuthorId = userId,
                PublishDate = DateTime.UtcNow
            };

            await this.db.Articles.AddAsync(article);
            await this.db.SaveChangesAsync();
        }

        public async Task<int> TotalAsync()
            => await this.db.Articles.CountAsync();
    }
}
