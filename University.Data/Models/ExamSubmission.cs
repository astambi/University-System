namespace University.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ExamSubmission
    {
        public int Id { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(DataConstants.FileMaxLengthInBytes)]
        public byte[] FileSubmission { get; set; }

        [Required]
        public string StudentId { get; set; }

        public User Student { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }
    }
}
