namespace University.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using University.Common.Infrastructure.Extensions;

    public class Article : IValidatableObject
    {
        public int Id { get; set; }

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

        public DateTime PublishDate { get; set; }

        public string AuthorId { get; set; }

        public User Author { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //var isFutureDate = DateTime.Now.Date < this.PublishDate.ToLocalTime().Date;
            var isFutureDate = this.PublishDate.IsUpcoming();
            if (isFutureDate)
            {
                yield return new ValidationResult(DataConstants.ArticlePublishDate,
                    new[] { nameof(this.PublishDate) });
            }
        }
    }
}
