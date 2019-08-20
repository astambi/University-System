namespace University.Web.Areas.Blog.Models.Articles
{
    using System.ComponentModel.DataAnnotations;
    using University.Data;

    public class ArticleFormModel
    {
        [Required]
        [StringLength(DataConstants.ArticleTitleMaxLength,
            ErrorMessage = DataConstants.StringMinMaxLength,
            MinimumLength = DataConstants.ArticleTitleMinLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(DataConstants.ArticleContentMaxLength,
            ErrorMessage = DataConstants.StringMinMaxLength,
            MinimumLength = DataConstants.ArticleContentMinLength)]
        public string Content { get; set; }
    }
}
