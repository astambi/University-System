namespace University.Services.Blog.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Blog.Models;

    public class ArticleService : IArticleService
    {
        private const int InvalidId = int.MinValue;

        private readonly UniversityDbContext db;
        private readonly IHtmlService htmlService;
        private readonly IMapper mapper;

        public ArticleService(
            UniversityDbContext db,
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
            => await this.GetQuerableBySearchKeyword(search)
            .OrderByDescending(a => a.PublishDate)
            .ProjectTo<ArticleListingServiceModel>(this.mapper.ConfigurationProvider)
            .GetPageItems(page, pageSize)
            .ToListAsync();

        public async Task<int> CreateAsync(string title, string rawHtmlContent, string authorId)
        {
            var userExists = this.db.Users.Any(u => u.Id == authorId);
            if (!userExists
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(rawHtmlContent))
            {
                return InvalidId;
            }

            var sanitizedContent = this.htmlService.Sanitize(rawHtmlContent); // sanitized html

            var article = new Article
            {
                Title = title.Trim(),
                Content = sanitizedContent,
                AuthorId = authorId,
                PublishDate = DateTime.UtcNow
            };

            await this.db.Articles.AddAsync(article);
            var result = await this.db.SaveChangesAsync();

            return article.Id;
        }

        public async Task<bool> ExistsAsync(int id)
            => await this.db.Articles.AnyAsync(a => a.Id == id);

        public async Task<bool> ExistsForAuthorAsync(int articleId, string authorId)
            => await this.db.Articles.AnyAsync(a => a.Id == articleId && a.AuthorId == authorId);

        public async Task<ArticleDetailsServiceModel> GetByIdAsync(int id)
            => await this.db
            .Articles
            .Where(a => a.Id == id)
            .ProjectTo<ArticleDetailsServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<ArticleEditServiceModel> GetByIdToEditAsync(int id, string authorId)
            => await this.db
            .Articles
            .Where(a => a.Id == id)
            .Where(a => a.AuthorId == authorId)
            .ProjectTo<ArticleEditServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<bool> RemoveAsync(int id, string authorId)
        {
            var article = await this.db.Articles.FindAsync(id);
            if (article == null
                || article.AuthorId != authorId)
            {
                return false;
            }

            this.db.Articles.Remove(article);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> TotalAsync(string search = null)
            => await this.GetQuerableBySearchKeyword(search).CountAsync();

        public async Task<bool> UpdateAsync(int id, string title, string rawHtmlContent, string authorId)
        {
            var article = this.db.Articles.Find(id);
            if (article == null
                || article.AuthorId != authorId
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(rawHtmlContent))
            {
                return false;
            }

            article.Title = title.Trim();
            article.Content = this.htmlService.Sanitize(rawHtmlContent); // sanitized html

            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        private IQueryable<Article> GetQuerableBySearchKeyword(string search)
        {
            var articles = this.db.Articles.AsQueryable();
            if (string.IsNullOrWhiteSpace(search))
            {
                return articles;
            }

            var searchLower = search.ToLower();

            return articles
                .Where(a => a.Title.ToLower().Contains(searchLower)
                    || a.Content.ToLower().Contains(searchLower));
        }
    }
}
