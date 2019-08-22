namespace University.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Certificate
    {
        public string Id { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        [Range(DataConstants.GradeBgMinValue, DataConstants.GradeBgMaxValue,
            ErrorMessage = DataConstants.RangeMinMaxValues)]
        public decimal GradeBg { get; set; }

        [Required]
        public string StudentId { get; set; }

        public User Student { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }
    }
}
