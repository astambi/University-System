namespace LearningSystem.Services.Blog.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Common.Infrastructure.Extensions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Services.Blog.Models;
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

        public async Task<IEnumerable<ArticleListingServiceModel>> AllAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.mapper
            .ProjectTo<ArticleListingServiceModel>(this.GetQuerableBySearchKeyword(search))
            .OrderByDescending(a => a.PublishDate)
            .GetPageItems(page, pageSize)
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

        public async Task<ArticleDetailsServiceModel> GetByIdAsync(int id)
            => await this.mapper
            .ProjectTo<ArticleDetailsServiceModel>(this.db.Articles)
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();

        public async Task<int> TotalAsync(string search = null)
            => await this.GetQuerableBySearchKeyword(search).CountAsync();

        private IQueryable<Article> GetQuerableBySearchKeyword(string search)
        {
            var articlesAsQuerable = this.db.Articles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keywordPattern = $@"\b{search.Trim()}\b"; // whole words only
                var options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

                articlesAsQuerable = articlesAsQuerable
                    .Where(a => Regex.IsMatch(a.Title, keywordPattern, options)
                             || Regex.IsMatch(a.Content, keywordPattern, options))
                    .AsQueryable();
            }

            return articlesAsQuerable;
        }
    }
}
