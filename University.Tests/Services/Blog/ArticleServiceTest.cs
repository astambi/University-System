namespace University.Tests.Services.Blog
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Blog;
    using University.Services.Blog.Implementations;
    using University.Services.Blog.Models;
    using University.Tests.Mocks;
    using Xunit;

    public class ArticleServiceTest
    {
        private const int ArticleValidId = 111;
        private const int ArticleInvalidId = 1000;

        private const string ArticleContent = "Article content";

        private const string ArticleTitle = "Article title";
        private const string ArticleTitleEdit = "  New title  ";

        private const string AuthorValidId = "AuthorValidId";
        private const string AuthorInvalidId = "AuthorInvalidId";

        private const string AuthorName = "Author name";
        private const string AuthorUserName = "Author username";
        private const string AuthorEmail = "email@gmail.com";

        private const string SanitizedContent = "Sanitized HTML content";

        private const string Search = "COde";
        private const string SearchInvalid = "XXX";

        private const int Precion = 50;

        private readonly DateTime PublishDate = new DateTime(2019, 1, 15);

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_GivenInvalidId()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.ExistsAsync(ArticleInvalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_GivenValidId()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.ExistsAsync(ArticleValidId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(ArticleInvalidId, AuthorValidId)]
        [InlineData(ArticleValidId, AuthorInvalidId)]
        public async Task ExistsForAuthorAsync_ShouldReturnFalse_GivenInvalidInput(int articleId, string authorId)
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.ExistsForAuthorAsync(articleId, authorId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsForAuthorAsync_ShouldReturnTrue_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.ExistsForAuthorAsync(ArticleValidId, AuthorValidId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("title", "content", AuthorInvalidId)]
        [InlineData("     ", "content", AuthorValidId)]
        [InlineData("title", "       ", AuthorValidId)]
        public async Task CreateAsync_ShouldReturnFalse_GivenInvalidInput(string title, string content, string authorId)
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            var countBefore = db.Articles.Count();

            // Act
            var result = await articleService.CreateAsync(title, content, authorId);
            var countAfter = db.Articles.Count();

            // Assert
            Assert.True(result < 0);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            var countBefore = db.Articles.Count();

            // Act
            var result = await articleService.CreateAsync(ArticleTitleEdit, SanitizedContent, AuthorValidId);
            var countAfter = db.Articles.Count();
            var resultArticle = db.Articles.Find(result);

            // Assert
            Assert.True(result > 0);
            Assert.Equal(1, countAfter - countBefore);

            Assert.NotNull(resultArticle);
            Assert.Equal(result, resultArticle.Id);
            Assert.Equal(AuthorValidId, resultArticle.AuthorId);
            Assert.Equal(ArticleTitleEdit.Trim(), resultArticle.Title);

            Assert.Equal(SanitizedContent, resultArticle.Content); // sanitized content

            resultArticle.PublishDate.Should().BeCloseTo(DateTime.UtcNow, Precion);
        }

        [Theory]
        [InlineData("title", "content", AuthorInvalidId)]
        [InlineData("     ", "content", AuthorValidId)]
        [InlineData("title", "       ", AuthorValidId)]
        public async Task UpdateAsync_ShouldNotChangeEntity_GivenInvalidInput(string title, string content, string authorId)
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            var articleBefore = db.Articles.Find(ArticleValidId);

            // Act
            var result = await articleService.UpdateAsync(ArticleValidId, title, content, authorId);
            var articleAfter = db.Articles.Find(ArticleValidId);

            // Assert
            Assert.False(result);
            Assert.Equal(articleBefore, articleAfter);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSaveCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.UpdateAsync(ArticleValidId, ArticleTitleEdit, SanitizedContent, AuthorValidId);
            var resultArticle = db.Articles.Find(ArticleValidId);

            // Assert
            Assert.True(result);

            Assert.Equal(AuthorValidId, resultArticle.AuthorId);
            Assert.Equal(ArticleTitleEdit.Trim(), resultArticle.Title);
            Assert.Equal(SanitizedContent, resultArticle.Content); // sanitized content
        }

        [Theory]
        [InlineData(ArticleInvalidId, AuthorValidId)]
        [InlineData(ArticleValidId, AuthorInvalidId)]
        public async Task RemoveAsync_ShouldReturnFalse_GivenInvalidInput(int articleId, string authorId)
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            var countBefore = db.Articles.Count();

            // Act
            var result = await articleService.RemoveAsync(articleId, authorId);
            var countAfter = db.Articles.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveCorrectEntity_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            var countBefore = db.Articles.Count();

            // Act
            var result = await articleService.RemoveAsync(ArticleValidId, AuthorValidId);
            var countAfter = db.Articles.Count();
            var article = db.Articles.Find(ArticleValidId);

            // Assert
            Assert.True(result);
            Assert.Equal(-1, countAfter - countBefore);
            Assert.Null(article);
        }

        [Theory]
        [InlineData(ArticleInvalidId, AuthorValidId)]
        [InlineData(ArticleValidId, AuthorInvalidId)]
        public async Task GetByIdToEditAsync_ShouldReturnNull_GivenInvalidInput(int articleId, string authorId)
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.GetByIdToEditAsync(articleId, authorId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdToEditAsync_ShouldReturnCorrectData_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.GetByIdToEditAsync(ArticleValidId, AuthorValidId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArticleEditServiceModel>(result);

            Assert.Equal(ArticleTitle, result.Title);
            Assert.Equal(ArticleContent, result.Content);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.GetByIdAsync(ArticleInvalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareArticles();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.GetByIdAsync(ArticleValidId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ArticleDetailsServiceModel>(result);

            Assert.Equal(ArticleTitle, result.Title);
            Assert.Equal(ArticleContent, result.Content);
            Assert.Equal(this.PublishDate, result.PublishDate);

            Assert.Equal(AuthorValidId, result.AuthorId);
            Assert.Equal(AuthorName, result.AuthorName);
            Assert.Equal(AuthorUserName, result.AuthorUsername);
            Assert.Equal(AuthorEmail, result.AuthorEmail);
        }

        [Theory]
        [InlineData("  ", 10)] // all
        [InlineData(null, 10)] // all
        [InlineData(Search, 6)]
        public async Task TotalAsync_ShouldReturnCorrectData_Given(string search, int expectedResult)
        {
            // Arrange
            var db = await this.PrepareArticlesToSearch();
            var articleService = this.InitializeArticleService(db);

            // Act
            var result = await articleService.TotalAsync(search);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task AllAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareArticlesToSearch();
            var articleService = this.InitializeArticleService(db);

            // Act
            var resultNullSearch = await articleService.AllAsync(null);
            var result = await articleService.AllAsync(Search);
            var result1Of2 = await articleService.AllAsync(Search, 1, 2);
            var result2Of2 = await articleService.AllAsync(Search, 2, 2);

            var resultInvalidPageMax = await articleService.AllAsync(Search, 222, 2); // invalid page
            var resultInvalidPageMin = await articleService.AllAsync(Search, -222, 2); // page 1 
            var resultInvalidPageSize = await articleService.AllAsync(Search, 2, -2); // page size 1 

            // Assert
            Assert.Equal(new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }, resultNullSearch.Select(a => a.Id));
            Assert.Equal(new[] { 6, 5, 4, 3, 2, 1 }, result.Select(a => a.Id));
            Assert.Equal(new[] { 6, 5 }, result1Of2.Select(a => a.Id));
            Assert.Equal(new[] { 4, 3 }, result2Of2.Select(a => a.Id));

            Assert.Empty(resultInvalidPageMax);
            Assert.Equal(result1Of2.Select(a => a.Id), resultInvalidPageMin.Select(a => a.Id));
            Assert.Equal(new[] { 5 }, resultInvalidPageSize.Select(a => a.Id)); // page 2, size 1

            var actualArticle = resultInvalidPageSize.First();
            var expectedArticle = db.Articles.Find(actualArticle.Id);

            Assert.Equal(expectedArticle.PublishDate, actualArticle.PublishDate);
            Assert.Equal(expectedArticle.Title, actualArticle.Title);
            Assert.Equal(expectedArticle.Author.Name, actualArticle.AuthorName);
        }

        private async Task<UniversityDbContext> PrepareArticles()
        {
            var user1 = new User { Id = AuthorValidId, Name = AuthorName, UserName = AuthorUserName, Email = AuthorEmail };

            var article1 = new Article { Id = ArticleValidId, AuthorId = AuthorValidId, Title = ArticleTitle, Content = ArticleContent, PublishDate = PublishDate };

            var article2 = new Article { Id = 222, AuthorId = AuthorValidId, Title = ArticleTitle, Content = ArticleContent, PublishDate = PublishDate };
            var article3 = new Article { Id = 333, AuthorId = AuthorValidId, Title = ArticleTitle, Content = ArticleContent, PublishDate = PublishDate };
            var article4 = new Article { Id = 444, AuthorId = AuthorValidId, Title = ArticleTitle, Content = ArticleContent, PublishDate = PublishDate };

            var db = Tests.InitializeDatabase();
            await db.Articles.AddRangeAsync(article1, article2, article3, article4);
            await db.Users.AddAsync(user1);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareArticlesToSearch()
        {
            var article1 = new Article { Id = 1, AuthorId = AuthorValidId, Title = $"{Search} AAAA", Content = "", PublishDate = this.PublishDate.AddDays(-9) };
            var article2 = new Article { Id = 2, AuthorId = AuthorValidId, Title = $"{Search.ToUpper()} AAAA", Content = "", PublishDate = this.PublishDate.AddDays(-8) };
            var article3 = new Article { Id = 3, AuthorId = AuthorValidId, Title = $"AAA {Search.ToLower()} bbb", Content = "", PublishDate = this.PublishDate.AddDays(-7) };
            var article4 = new Article { Id = 4, AuthorId = AuthorValidId, Content = $"{Search} AAAA", Title = "", PublishDate = this.PublishDate.AddDays(-6) };
            var article5 = new Article { Id = 5, AuthorId = AuthorValidId, Content = $"{Search.ToUpper()} AAAA", Title = "", PublishDate = this.PublishDate.AddDays(-5) };
            var article6 = new Article { Id = 6, AuthorId = AuthorValidId, Content = $"AAA {Search.ToLower()} bbb", Title = "", PublishDate = this.PublishDate.AddDays(-4) };

            var article7 = new Article { Id = 7, AuthorId = AuthorValidId, Title = $"AAA bbb{SearchInvalid} ccc", Content = "", PublishDate = this.PublishDate.AddDays(-3) };
            var article8 = new Article { Id = 8, AuthorId = AuthorValidId, Title = $"AAA {SearchInvalid}bbb ccc", Content = "", PublishDate = this.PublishDate.AddDays(-2) };
            var article9 = new Article { Id = 9, AuthorId = AuthorValidId, Content = $"AAA bbb{SearchInvalid} ccc", Title = "", PublishDate = this.PublishDate.AddDays(-1) };
            var article10 = new Article { Id = 10, AuthorId = AuthorValidId, Content = $"AAA {SearchInvalid}bbb ccc", Title = "", PublishDate = this.PublishDate.AddDays(0) };

            /// Searching for whole words only => removed as EF 3.0 does not evaluate complex queries on the client
            //var article7 = new Article { Id = 7, AuthorId = AuthorValidId, Title = $"AAA bbb{Search} ccc", Content = "", PublishDate = this.PublishDate.AddDays(-3) };
            //var article8 = new Article { Id = 8, AuthorId = AuthorValidId, Title = $"AAA {Search}bbb ccc", Content = "", PublishDate = this.PublishDate.AddDays(-2) };
            //var article9 = new Article { Id = 9, AuthorId = AuthorValidId, Content = $"AAA bbb{Search} ccc", Title = "", PublishDate = this.PublishDate.AddDays(-1) };
            //var article10 = new Article { Id = 10, AuthorId = AuthorValidId, Content = $"AAA {Search}bbb ccc", Title = "", PublishDate = this.PublishDate.AddDays(0) };

            var author = new User { Id = AuthorValidId, Name = AuthorName };

            var db = Tests.InitializeDatabase();
            await db.Articles.AddRangeAsync(article1, article2, article3, article4, article5, article6, article7, article8, article9, article10);
            await db.Users.AddAsync(author);
            await db.SaveChangesAsync();

            return db;
        }

        private IArticleService InitializeArticleService(UniversityDbContext db)
        {
            var htmlService = HtmlServiceMock.GetMock;
            htmlService.Sanitize(SanitizedContent);

            return new ArticleService(db, htmlService.Object, Tests.Mapper);
        }
    }
}
