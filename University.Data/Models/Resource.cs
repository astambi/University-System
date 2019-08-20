namespace University.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Resource
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.ResourceNameMaxLength,
            ErrorMessage = DataConstants.StringMinMaxLength,
            MinimumLength = DataConstants.ResourceNameMinLength)]
        public string FileName { get; set; }

        [Required]
        [StringLength(DataConstants.ContentTypeMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string ContentType { get; set; }

        [StringLength(DataConstants.ResourceMaxLengthInBytes,
            ErrorMessage = DataConstants.FileMaxLength)]
        public byte[] FileBytes { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }
    }
}
