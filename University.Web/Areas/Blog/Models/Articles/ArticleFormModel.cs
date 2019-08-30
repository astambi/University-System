namespace University.Web.Areas.Blog.Models.Articles
{
    using System.ComponentModel.DataAnnotations;
    using University.Common.Mapping;
    using University.Data;
    using University.Services.Blog.Models;
    using University.Web.Models;

    public class ArticleFormModel : IMapFrom<ArticleEditServiceModel>
    {
        public int Id { get; set; } = int.MinValue; // invalid Id for Create

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

        public FormActionEnum Action { get; set; } = FormActionEnum.Create; // default Create
    }
}
