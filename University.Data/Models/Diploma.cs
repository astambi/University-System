namespace University.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Diploma
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string StudentId { get; set; }

        public User Student { get; set; }

        public int CurriculumId { get; set; }

        public Curriculum Curriculum { get; set; }
    }
}
