namespace LearningSystem.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Course : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.CourseNameMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.CourseDescriptionMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Description { get; set; }

        [Required]
        public string TrainerId { get; set; }

        public User Trainer { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public ICollection<Resource> Resources { get; set; } = new List<Resource>();

        public ICollection<StudentCourse> Students { get; set; } = new List<StudentCourse>();

        public ICollection<ExamSubmission> ExamSubmissions { get; set; } = new List<ExamSubmission>();

        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasStartBeforeToday = this.StartDate.ToLocalTime().Date < DateTime.Now.Date;
            var hasEndBeforeStart = this.StartDate < this.EndDate;

            if (hasStartBeforeToday)
            {
                yield return new ValidationResult(DataConstants.CourseStartDate,
                    new[] { nameof(this.StartDate) });
            }

            if (hasEndBeforeStart)
            {
                yield return new ValidationResult(DataConstants.CourseEndDate,
                    new[] { nameof(this.EndDate) });
            }
        }
    }
}
