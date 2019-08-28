namespace University.Web.Models.Resources
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;
    using University.Data;

    public class ResourceCreateFormModel : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }

        [Required]
        public IFormFile ResourceFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.ResourceFile.Length == 0
                || this.ResourceFile.Length > DataConstants.ResourceMaxLengthInBytes)
            {
                yield return new ValidationResult(
                    string.Format(WebConstants.FileSizeInvalidMsg, DataConstants.ResourceFileMaxLengthInMb),
                    new[] { nameof(this.ResourceFile) });
            }
        }
    }
}
