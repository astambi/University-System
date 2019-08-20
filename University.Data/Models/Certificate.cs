namespace University.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Certificate
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public Grade Grade { get; set; }

        [Required]
        public string StudentId { get; set; }

        public User Student { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }
    }
}
